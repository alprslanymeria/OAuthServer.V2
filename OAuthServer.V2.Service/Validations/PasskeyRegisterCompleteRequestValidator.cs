using FluentValidation;
using OAuthServer.V2.Core.DTOs.Passkeys;

namespace OAuthServer.V2.Service.Validations;

public class PasskeyRegisterCompleteRequestValidator : AbstractValidator<PasskeyRegisterCompleteRequest>
{
    public PasskeyRegisterCompleteRequestValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("RequestId is required.");
    }
}
