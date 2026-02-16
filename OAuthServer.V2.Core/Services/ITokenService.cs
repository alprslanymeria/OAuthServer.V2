using OAuthServer.V2.Core.Configuration;
using OAuthServer.V2.Core.DTOs.Client;
using OAuthServer.V2.Core.DTOs.RefreshToken;
using OAuthServer.V2.Core.Models;

namespace OAuthServer.V2.Core.Services;

public interface ITokenService
{
    // THE METHODS IN THIS INTERFACE ARE IMPLEMENTED IN THE SERVICE LAYER.
    // THE METHODS IN THIS INTERFACE CAN BE USED IN THE SERVICE OR PRESENTATION (API) LAYER.
    // THE DATA RETURNED FROM THE METHODS CAN BE USED IN THE SERVICE OR PRESENTATION (API) LAYER.

    TokenResponse CreateToken(User user);
    ClientTokenResponse CreateTokenByClient(Client client);
}