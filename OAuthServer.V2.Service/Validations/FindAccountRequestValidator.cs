using FluentValidation;
using OAuthServer.V2.Core.DTOs.ForgotPassword;

namespace OAuthServer.V2.Service.Validations;

public class FindAccountRequestValidator : AbstractValidator<FindAccountRequest>
{
    public FindAccountRequestValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Username, email, or phone number is required.")
            .MaximumLength(256).WithMessage("Identifier must not exceed 256 characters.");
    }
}
