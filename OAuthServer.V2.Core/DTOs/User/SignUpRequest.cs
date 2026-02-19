using OAuthServer.V2.Core.Services;

namespace OAuthServer.V2.Core.DTOs.User;

public record SignUpRequest(

    string FirstName,
    string? Email,
    string? PhoneNumber,
    DateTime BirthDate,
    string Password,
    IFileUpload? Image,
    string? UserName
    );
