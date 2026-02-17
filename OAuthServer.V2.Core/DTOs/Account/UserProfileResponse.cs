namespace OAuthServer.V2.Core.DTOs.Account;

public record UserProfileResponse(

    string? UserName,
    string? Email,
    string? PhoneNumber,
    string? Image,
    string FirstName,
    DateTime BirthDate,
    string Locale,
    bool IsActive,
    bool EmailConfirmed,
    bool PhoneNumberConfirmed);
