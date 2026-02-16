namespace OAuthServer.V2.Core.DTOs.Client;

public record ClientTokenResponse(

    string AccessToken,
    DateTime AccessTokenExpiration);