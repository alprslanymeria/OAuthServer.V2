using System.Net;

namespace OAuthServer.V2.Core.Exceptions;

public class UnauthorizedException(string message) : AppException(message, HttpStatusCode.Unauthorized);
