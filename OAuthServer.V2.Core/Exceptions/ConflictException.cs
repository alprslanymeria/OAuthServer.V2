using System.Net;

namespace OAuthServer.V2.Core.Exceptions;

public class ConflictException(string message) : AppException(message, HttpStatusCode.Conflict);
