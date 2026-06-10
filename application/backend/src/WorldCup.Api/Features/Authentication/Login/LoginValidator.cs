using FluentValidation;

namespace WorldCup.Api.Features.Authentication.Login;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
