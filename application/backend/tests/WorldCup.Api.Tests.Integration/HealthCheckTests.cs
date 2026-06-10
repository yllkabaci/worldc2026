using System.Net;
using FluentAssertions;
using WorldCup.Tests.Helpers;
using Xunit;

namespace WorldCup.Api.Tests.Integration;

public class HealthCheckTests(WorldCupApiFactory factory) : IClassFixture<WorldCupApiFactory>
{
    [Fact]
    public async Task Healthz_ReturnsOk()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/healthz");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
