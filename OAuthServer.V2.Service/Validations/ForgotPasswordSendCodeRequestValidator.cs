using FluentValidation;
using OAuthServer.V2.Core.DTOs.ForgotPassword;

namespace OAuthServer.V2.Service.Validations;

public class ForgotPasswordSendCodeRequestValidator : AbstractValidator<ForgotPasswordSendCodeRequest>
{
    public ForgotPasswordSendCodeRequestValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Identifier is required.");

        RuleFor(x => x.DeliveryMethod)
            .IsInEnum().WithMessage("Invalid delivery method. Use 1 for Email or 2 for SMS.");
    }
}
