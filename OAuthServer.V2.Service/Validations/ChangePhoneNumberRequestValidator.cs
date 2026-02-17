using FluentValidation;
using OAuthServer.V2.Core.DTOs.Account;

namespace OAuthServer.V2.Service.Validations;

public class ChangePhoneNumberRequestValidator : AbstractValidator<ChangePhoneNumberRequest>
{
    public ChangePhoneNumberRequestValidator()
    {
        RuleFor(x => x.NewPhoneNumber)
            .NotEmpty().WithMessage("New phone number is required.")
            .Matches(@"^\+[1-9]\d{6,14}$").WithMessage("Phone number must be in E.164 format (e.g., +905551234567).");
    }
}
