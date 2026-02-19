using FluentValidation;
using OAuthServer.V2.Core.DTOs.Passkeys;

namespace OAuthServer.V2.Service.Validations;

public class PasskeyLoginCompleteRequestValidator : AbstractValidator<PasskeyLoginCompleteRequest>
{
    public PasskeyLoginCompleteRequestValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("RequestId is required.");
    }
}
