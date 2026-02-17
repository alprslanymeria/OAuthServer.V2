using FluentValidation;
using OAuthServer.V2.Core.DTOs.Account;

namespace OAuthServer.V2.Service.Validations;

public class DeactivateAccountRequestValidator : AbstractValidator<DeactivateAccountRequest>
{
    public DeactivateAccountRequestValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required to deactivate your account.");
    }
}
