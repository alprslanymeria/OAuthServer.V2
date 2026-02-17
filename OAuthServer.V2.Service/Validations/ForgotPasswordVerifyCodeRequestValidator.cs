using FluentValidation;
using OAuthServer.V2.Core.DTOs.ForgotPassword;

namespace OAuthServer.V2.Service.Validations;

public class ForgotPasswordVerifyCodeRequestValidator : AbstractValidator<ForgotPasswordVerifyCodeRequest>
{
    public ForgotPasswordVerifyCodeRequestValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Identifier is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("Verification code must contain only digits.");

        RuleFor(x => x.DeliveryMethod)
            .IsInEnum().WithMessage("Invalid delivery method.");
    }
}
