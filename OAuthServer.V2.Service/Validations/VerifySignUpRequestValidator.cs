using FluentValidation;
using OAuthServer.V2.Core.DTOs.Verification;

namespace OAuthServer.V2.Service.Validations;

public class VerifySignUpRequestValidator : AbstractValidator<VerifySignUpRequest>
{
    public VerifySignUpRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email) || !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Email or phone number is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("Verification code must contain only digits.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+[1-9]\d{6,14}$").WithMessage("Phone number must be in E.164 format.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
