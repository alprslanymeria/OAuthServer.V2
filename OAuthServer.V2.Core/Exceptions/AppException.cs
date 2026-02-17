using System.Net;

namespace OAuthServer.V2.Core.Exceptions;

public abstract class AppException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
