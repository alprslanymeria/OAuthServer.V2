using FluentValidation;
using OAuthServer.V2.Core.DTOs.Account;

namespace OAuthServer.V2.Service.Validations;

public class VerifyChangePhoneNumberRequestValidator : AbstractValidator<VerifyChangePhoneNumberRequest>
{
    public VerifyChangePhoneNumberRequestValidator()
    {
        RuleFor(x => x.NewPhoneNumber)
            .NotEmpty().WithMessage("New phone number is required.")
            .Matches(@"^\+[1-9]\d{6,14}$").WithMessage("Phone number must be in E.164 format.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("Verification code must contain only digits.");
    }
}
