using Microsoft.AspNetCore.Diagnostics;
using OAuthServer.V2.Core.Common;
using OAuthServer.V2.Core.Exceptions;
using System.Net;

namespace OAuthServer.V2.API.ExceptionHandlers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ServiceResult errorResponse;
        HttpStatusCode statusCode;

        switch (exception)
        {
            // BUSINESS EXCEPTION - SUPPORTS MULTIPLE ERROR MESSAGES (E.G., IDENTITY ERRORS)
            case BusinessException businessEx:
                statusCode = businessEx.StatusCode;
                errorResponse = ServiceResult.Fail(businessEx.Errors, statusCode);
                _logger.LogWarning("BUSINESS ERROR: {Errors}", string.Join(" | ", businessEx.Errors));
                break;

            // ALL OTHER CUSTOM APP EXCEPTIONS (NOTFOUND, FORBIDDEN, UNAUTHORIZED, CONFLICT)
            case AppException appEx:
                statusCode = appEx.StatusCode;
                errorResponse = ServiceResult.Fail(appEx.Message, statusCode);
                _logger.LogWarning("APPLICATION ERROR [{StatusCode}]: {Message}", statusCode, appEx.Message);
                break;

            // UNEXPECTED / UNHANDLED EXCEPTIONS - NEVER EXPOSE INTERNAL DETAILS TO CLIENT
            default:
                statusCode = HttpStatusCode.InternalServerError;
                errorResponse = ServiceResult.Fail("An unexpected error occurred.", statusCode);
                _logger.LogError(exception, "UNHANDLED EXCEPTION");
                break;
        }

        httpContext.Response.StatusCode = (int)statusCode;
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true;
    }
}
