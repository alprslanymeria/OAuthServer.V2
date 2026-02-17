using System.Net;

namespace OAuthServer.V2.Core.Exceptions;

public class ForbiddenException(string message) : AppException(message, HttpStatusCode.Forbidden);
