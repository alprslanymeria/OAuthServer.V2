using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OAuthServer.V2.Core.Common;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace OAuthServer.V2.API.Filters;

public class FluentValidationFilter : IFluentValidationAutoValidationResultFactory
{
    public Task<IActionResult?> CreateActionResult(ActionExecutingContext context, ValidationProblemDetails validationProblemDetails, IDictionary<IValidationContext, ValidationResult> validationResults)
    {
        var errors = context.ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage)
                            .ToList();

        var responseModel = ServiceResult.Fail(errors);

        return Task.FromResult<IActionResult?>(new BadRequestObjectResult(responseModel));
    }
}