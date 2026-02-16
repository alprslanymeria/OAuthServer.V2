namespace OAuthServer.V2.Core.DTOs.User;

public record SignInRequest(

    string? Email,
    string? UserName,
    string? PhoneNumber,
    string Password);