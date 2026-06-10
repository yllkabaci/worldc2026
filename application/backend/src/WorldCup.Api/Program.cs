using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WorldCup.Api.Common.Behaviors;
using WorldCup.Api.Common.Errors;
using WorldCup.Api.Common.Modules;
using WorldCup.Api.Common.Services;
using WorldCup.Domain.Abstractions;
using WorldCup.Infrastructure;
using WorldCup.Infrastructure.Identity;

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

// --- Infrastructure ---
builder.Services.AddInfrastructure(connectionString, jwt);

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
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddFeatureModules(typeof(Program).Assembly);

var app = builder.Build();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/healthz");
app.MapHealthChecks("/readyz");
app.MapFeatureEndpoints(typeof(Program).Assembly);

app.Run();

/// <summary>Exposed for WebApplicationFactory in integration tests.</summary>
public partial class Program { }
