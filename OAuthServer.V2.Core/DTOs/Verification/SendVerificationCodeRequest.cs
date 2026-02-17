namespace OAuthServer.V2.Core.DTOs.Verification;

public record SendVerificationCodeRequest(

    string? Email,
    string? PhoneNumber);
