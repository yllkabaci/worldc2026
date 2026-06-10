using FluentAssertions;
using WorldCup.Domain.Users;
using Xunit;

namespace WorldCup.Domain.Tests.Unit.Users;

public class UserTests
{
    [Fact]
    public void Register_CreatesActiveUserWithUserRole_AndNormalisesEmail()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var user = User.Register("  Jane@Example.com ", "hashed", now);

        // Assert
        user.Email.Should().Be("jane@example.com");
        user.Role.Should().Be(Role.User);
        user.Status.Should().Be(AccountStatus.Active);
        user.IsActive.Should().BeTrue();
        user.DomainEvents.Should().ContainSingle(e => e is UserRegisteredEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_RejectsEmptyEmail(string email)
    {
        // Act
        var act = () => User.Register(email, "hashed", DateTimeOffset.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
