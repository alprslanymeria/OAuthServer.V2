namespace OAuthServer.V2.Core.DTOs.Client;

public record ClientSignInRequest(

    string ClientId,
    string ClientSecret);