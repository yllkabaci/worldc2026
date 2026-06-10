using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorldCup.Infrastructure.Persistence;
using WorldCup.Tests.Helpers;
using Xunit;

namespace WorldCup.Api.Tests.Integration.Authentication;

public class AuthEndpointsTests(WorldCupApiFactory factory) : IClassFixture<WorldCupApiFactory>
{
    private sealed record Credentials(string email, string password);
    private sealed record Envelope<T>(bool success, T data);
    private sealed record RegisterData(string userId, string email);
    private sealed record LoginData(string token);
    private sealed record MeData(string userId, string email, string[] roles);

    private static Credentials NewUser() => new($"user{Guid.NewGuid():N}@example.com", "Password1!");

    private static async Task<string> RegisterAndLoginAsync(HttpClient client, Credentials creds)
    {
        (await client.PostAsJsonAsync("/api/auth/register", creds)).EnsureSuccessStatusCode();
        var login = await client.PostAsJsonAsync("/api/auth/login", creds);
        login.EnsureSuccessStatusCode();
        return (await login.Content.ReadFromJsonAsync<Envelope<LoginData>>())!.data.token;
    }

    private static string DecodeJwtPayload(string token)
    {
        var part = token.Split('.')[1].Replace('-', '+').Replace('_', '/');
        part = part.PadRight(part.Length + (4 - part.Length % 4) % 4, '=');
        return Encoding.UTF8.GetString(Convert.FromBase64String(part));
    }

    [Fact]
    public async Task Register_ThenLogin_ReturnsTokenWithExpectedClaims()
    {
        // Arrange
        var client = factory.CreateClient();
        var creds = NewUser();

        // Act
        var register = await client.PostAsJsonAsync("/api/auth/register", creds);
        var login = await client.PostAsJsonAsync("/api/auth/login", creds);

        // Assert
        register.StatusCode.Should().Be(HttpStatusCode.OK);
        (await register.Content.ReadFromJsonAsync<Envelope<RegisterData>>())!.data.email.Should().Be(creds.email);

        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = (await login.Content.ReadFromJsonAsync<Envelope<LoginData>>())!.data.token;
        token.Should().NotBeNullOrWhiteSpace();

        var payload = DecodeJwtPayload(token);
        payload.Should().Contain(creds.email);
        payload.Should().Contain("User");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var client = factory.CreateClient();
        var creds = NewUser();
        (await client.PostAsJsonAsync("/api/auth/register", creds)).EnsureSuccessStatusCode();

        (await client.PostAsJsonAsync("/api/auth/register", creds)).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_DuplicateEmailDifferentCase_Returns409()
    {
        var client = factory.CreateClient();
        var local = $"user{Guid.NewGuid():N}";
        (await client.PostAsJsonAsync("/api/auth/register", new Credentials($"{local}@example.com", "Password1!"))).EnsureSuccessStatusCode();

        var dup = await client.PostAsJsonAsync("/api/auth/register", new Credentials($"{local.ToUpperInvariant()}@EXAMPLE.com", "Password1!"));
        dup.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400()
    {
        var client = factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/register", new Credentials($"weak{Guid.NewGuid():N}@example.com", "weak"));
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_UnknownEmail_Returns401()
    {
        var client = factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/login", NewUser());
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var client = factory.CreateClient();
        var creds = NewUser();
        (await client.PostAsJsonAsync("/api/auth/register", creds)).EnsureSuccessStatusCode();

        var res = await client.PostAsJsonAsync("/api/auth/login", creds with { password = "WrongPass1!" });
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_BlockedUser_Returns401()
    {
        // Arrange
        var client = factory.CreateClient();
        var creds = NewUser();
        (await client.PostAsJsonAsync("/api/auth/register", creds)).EnsureSuccessStatusCode();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.FirstAsync(u => u.Email == creds.email);
            user.Block();
            await db.SaveChangesAsync();
        }

        // Act
        var res = await client.PostAsJsonAsync("/api/auth/login", creds);

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var client = factory.CreateClient();
        var res = await client.GetAsync("/api/auth/me");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_WithToken_ReturnsCurrentUser()
    {
        // Arrange
        var client = factory.CreateClient();
        var creds = NewUser();
        var token = await RegisterAndLoginAsync(client, creds);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var res = await client.GetAsync("/api/auth/me");

        // Assert
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = (await res.Content.ReadFromJsonAsync<Envelope<MeData>>())!.data;
        me.email.Should().Be(creds.email);
        me.roles.Should().Contain("User");
    }
}
