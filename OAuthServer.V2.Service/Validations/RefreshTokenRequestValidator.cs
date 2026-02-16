using FluentValidation;
using OAuthServer.V2.Core.DTOs.RefreshToken;

namespace OAuthServer.V2.Service.Validations;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotNull().WithMessage("REFRESH TOKEN REQUIRED")
            .NotEmpty().WithMessage("REFRESH TOKEN REQUIRED");
    }
}