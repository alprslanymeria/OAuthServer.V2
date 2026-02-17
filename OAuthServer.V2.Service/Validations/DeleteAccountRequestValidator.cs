using FluentValidation;
using OAuthServer.V2.Core.DTOs.Account;

namespace OAuthServer.V2.Service.Validations;

public class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountRequestValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required to delete your account.");
    }
}
