using System.Net;

namespace OAuthServer.V2.Core.Exceptions;

public class NotFoundException(string message) : AppException(message, HttpStatusCode.NotFound);
