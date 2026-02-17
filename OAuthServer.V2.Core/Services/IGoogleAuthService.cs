using OAuthServer.V2.Core.DTOs.GoogleAuth;
using OAuthServer.V2.Core.DTOs.RefreshToken;
using System.Security.Claims;

namespace OAuthServer.V2.Core.Services;

public interface IGoogleAuthService
{
    // THE METHODS IN THIS INTERFACE ARE IMPLEMENTED IN THE SERVICE LAYER.
    // THE METHODS IN THIS INTERFACE CAN BE USED IN THE SERVICE OR PRESENTATION (API) LAYER.

    void ValidateRedirectUri(string redirectUri);
    GoogleUserInfo ExtractUserInfo(ClaimsPrincipal principal);
    string BuildTokenRedirectUrl(string redirectUri, TokenResponse token);
}
