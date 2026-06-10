using FluentAssertions;
using WorldCup.Api.Common.Http;
using Xunit;

namespace WorldCup.Api.Tests.Unit;

public class ApiResponseTests
{
    [Fact]
    public void ToApiResponse_WrapsData_WithSuccessTrue()
    {
        // Arrange
        var payload = "hello";

        // Act
        var response = payload.ToApiResponse();

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be("hello");
    }

    [Fact]
    public void ToApiListResponse_MaterialisesCollection()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };

        // Act
        var response = items.ToApiListResponse();

        // Assert
        response.Data.Should().HaveCount(3);
    }
}
