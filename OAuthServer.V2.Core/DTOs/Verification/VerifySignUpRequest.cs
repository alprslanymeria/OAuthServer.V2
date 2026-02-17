namespace OAuthServer.V2.Core.DTOs.Verification;

public record VerifySignUpRequest(

    string? Email,
    string? PhoneNumber,
    string Code);
