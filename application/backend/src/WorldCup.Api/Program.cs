using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WorldCup.Api.Common.Behaviors;
using WorldCup.Api.Common.Errors;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using WorldCup.Api.Common.Modules;
using WorldCup.Api.Common.Observability;
using WorldCup.Api.Common.Services;
using WorldCup.Domain.Abstractions;
using WorldCup.Infrastructure;
using WorldCup.Infrastructure.ExternalApis;
using WorldCup.Infrastructure.Identity;
using WorldCup.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// --- Configuration ---
var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=worldcup.db";
var jwt = new JwtSettings
{
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "worldcup",
    Audience = builder.Configuration["Jwt:Audience"] ?? "worldcup",
    SigningKey = builder.Configuration["Jwt:SigningKey"] ?? "dev-only-signing-key-change-me-please-32chars",
    ExpiryMinutes = int.TryParse(builder.Configuration["Jwt:ExpiryMinutes"], out var m) ? m : 60 * 24
};

var footballData = new FootballDataOptions
{
    BaseUrl = builder.Configuration["FootballData:BaseUrl"] ?? "https://api.football-data.org/v4/",
    ApiToken = builder.Configuration["FootballData:ApiToken"] ?? "",
    CompetitionCode = builder.Configuration["FootballData:CompetitionCode"] ?? "WC"
};

// --- Infrastructure ---
builder.Services.AddInfrastructure(connectionString, jwt, footballData);

// --- MediatR + pipeline behaviors (order: Logging -> Validation -> UnitOfWork -> handler) ---
builder.Services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// --- Current user ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// --- JSON (camelCase, omit nulls, enums as strings) ---
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// --- AuthN / AuthZ ---
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("User", p => p.RequireRole("User", "Admin", "SuperAdmin"))
    .AddPolicy("Admin", p => p.RequireRole("Admin", "SuperAdmin"))
    .AddPolicy("SuperAdmin", p => p.RequireRole("SuperAdmin"));

// --- Errors, OpenAPI, health, CORS, features ---
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "World Cup 2026 Prediction API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Paste your JWT access token (without the 'Bearer ' prefix).",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    o.AddSecurityDefinition("Bearer", jwtScheme);
    o.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database", tags: ["ready"]);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(Telemetry.ServiceName))
    .WithTracing(tracing => tracing
        .AddSource(Telemetry.ServiceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddFeatureModules(typeof(Program).Assembly);

var app = builder.Build();

// Apply EF Core migrations on startup (the Testing host manages its own in-memory database).
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnostic, httpContext) =>
        diagnostic.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "anonymous");
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "World Cup 2026 API v1");
        o.DocumentTitle = "World Cup 2026 Prediction API";
    });
}
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/healthz");
app.MapHealthChecks("/readyz", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.MapFeatureEndpoints(typeof(Program).Assembly);

app.Run();

/// <summary>Exposed for WebApplicationFactory in integration tests.</summary>
public partial class Program { }
