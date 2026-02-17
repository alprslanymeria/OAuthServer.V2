using FluentValidation;
using OAuthServer.V2.Core.DTOs.User;

namespace OAuthServer.V2.Service.Validations;

public class SignInRequestValidator : AbstractValidator<SignInRequest>
{
    public SignInRequestValidator()
    {
        // AT LEAST ONE IDENTIFIER MUST BE PROVIDED
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Email)
                     || !string.IsNullOrWhiteSpace(x.UserName)
                     || !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("Email, username, or phone number is required.");

        // PASSWORD IS REQUIRED
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        // EMAIL FORMAT WHEN PROVIDED
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        // PHONE NUMBER FORMAT WHEN PROVIDED
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+[1-9]\d{6,14}$").WithMessage("Phone number must be in E.164 format.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
