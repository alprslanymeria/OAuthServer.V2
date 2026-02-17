using FluentValidation;
using OAuthServer.V2.Core.DTOs.Account;

namespace OAuthServer.V2.Service.Validations;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MinimumLength(2).WithMessage("First name must be at least 2 characters.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.")
            .When(x => x.FirstName is not null);

        RuleFor(x => x.BirthDate)
            .LessThan(DateTime.Now.AddYears(-13)).WithMessage("User must be at least 13 years old.")
            .GreaterThan(DateTime.Now.AddYears(-120)).WithMessage("Invalid birth date.")
            .When(x => x.BirthDate.HasValue);

        RuleFor(x => x.Locale)
            .MinimumLength(2).WithMessage("Locale must be at least 2 characters.")
            .MaximumLength(10).WithMessage("Locale must not exceed 10 characters.")
            .When(x => x.Locale is not null);
    }
}
