namespace OAuthServer.V2.Core.DTOs.RefreshToken;

public record TokenResponse(

    string AccessToken,
    DateTime AccessTokenExpiration,
    string RefreshToken,
    DateTime RefreshTokenExpiration);