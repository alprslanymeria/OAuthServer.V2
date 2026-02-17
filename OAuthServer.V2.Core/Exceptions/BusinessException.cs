using System.Net;

namespace OAuthServer.V2.Core.Exceptions;

public class BusinessException : AppException
{
    public List<string> Errors { get; }

    public BusinessException(string message) : base(message, HttpStatusCode.BadRequest)
    {
        Errors = [message];
    }

    public BusinessException(List<string> errors)
        : base(errors.Count > 0 ? errors[0] : "A business error occurred.", HttpStatusCode.BadRequest)
    {
        Errors = errors;
    }
}
