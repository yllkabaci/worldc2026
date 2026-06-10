using FluentAssertions;
using WorldCup.Domain.Exceptions;
using Xunit;

namespace WorldCup.Domain.Tests.Unit;

public class DomainExceptionTests
{
    private sealed class SampleException() : DomainException(ErrorCodes.NotFound, "missing");

    [Fact]
    public void DomainException_CarriesCodeAndMessage()
    {
        // Arrange / Act
        var ex = new SampleException();

        // Assert
        ex.Code.Should().Be(ErrorCodes.NotFound);
        ex.Message.Should().Be("missing");
    }
}
