using Microsoft.AspNetCore.Diagnostics;
using OAuthServer.V2.Core.Common;
using System.Net;

namespace OAuthServer.V2.API.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var errorAsDto = ServiceResult.Fail(exception.Message, HttpStatusCode.InternalServerError);

        httpContext.Response.StatusCode = HttpStatusCode.InternalServerError.GetHashCode();
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsJsonAsync(errorAsDto, cancellationToken);

        return true;

        // RETURN TRUE;     --> I'VE ADDRESSED THIS ERROR, I'LL RETURN THE RESPONSE MODEL I CREATED.
        // RETURN FALSE;    --> I'VE ADDRESSED THIS ERROR, I'VE DONE THE NECESSARY OPERATIONS. LET IT CONTINUE ITS JOURNEY.
    }
}
