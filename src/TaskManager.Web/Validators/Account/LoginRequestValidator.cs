using FluentValidation;
using TaskManager.Web.Models.Dtos.Account;

namespace TaskManager.Web.Validators.Account;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
