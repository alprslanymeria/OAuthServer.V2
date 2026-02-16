namespace OAuthServer.V2.Core.DTOs.User;

public record UserDto(

    string? UserName,
    string Email,
    string? Image,
    string FirstName,
    DateTime? BirthDate);