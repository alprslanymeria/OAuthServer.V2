using OAuthServer.V2.Core.Services;

namespace OAuthServer.V2.Core.DTOs.Account;

public record UpdateProfileRequest(

    string? FirstName,
    DateTime? BirthDate,
    string? Locale,
    IFileUpload? Image);
