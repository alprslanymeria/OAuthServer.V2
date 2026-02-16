using Microsoft.AspNetCore.Mvc;
using OAuthServer.V2.Core.Common;
using System.Net;

namespace OAuthServer.V2.API.Controllers;

public class BaseController : ControllerBase
{
    [NonAction]
    public IActionResult ActionResultInstance<T>(ServiceResult<T> response)
    {
        return response.Status switch
        {
            HttpStatusCode.NoContent => NoContent(),
            HttpStatusCode.Created => Created(response.UrlAsCreated, response),
            _ => new ObjectResult(response) { StatusCode = response.Status.GetHashCode() }
        };
    }

    [NonAction]
    public IActionResult ActionResultInstance(ServiceResult response)
    {
        return response.Status switch
        {
            HttpStatusCode.NoContent => new ObjectResult(null) { StatusCode = response.Status.GetHashCode() },
            _ => new ObjectResult(response) { StatusCode = response.Status.GetHashCode() }
        };
    }
}

// IN REST ARCHITECTURE, EVERY RESPONSE MUST HAVE A STATUS CODE BUT NOT NECESSARILY A BODY.
// WE USE "NON ACTION" ATTRIBUTE BECAUSE THIS IS A HELPER METHOD.