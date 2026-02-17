using Microsoft.Extensions.Configuration;
using OAuthServer.V2.Core.DTOs.GoogleAuth;
using OAuthServer.V2.Core.DTOs.RefreshToken;
using OAuthServer.V2.Core.Exceptions;
using OAuthServer.V2.Core.Services;
using System.Security.Claims;
using System.Web;

namespace OAuthServer.V2.Service.Services;

public class GoogleAuthService(IConfiguration configuration) : IGoogleAuthService
{
    private readonly IConfiguration _configuration = configuration;

    public void ValidateRedirectUri(string redirectUri)
    {
        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            throw new BusinessException("redirect_uri is required.");
        }

        var allowedUris = _configuration.GetSection("AllowedRedirectUris").Get<string[]>() ?? [];
        var uri = new Uri(redirectUri);

        var isAllowed = allowedUris.Any(allowed =>
        {
            var allowedUri = new Uri(allowed);
            return uri.Host == allowedUri.Host;
        });

        if (!isAllowed)
        {
            throw new BusinessException("Invalid redirect_uri.");
        }
    }

    public GoogleUserInfo ExtractUserInfo(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var email = principal.FindFirstValue(ClaimTypes.Email);
        var googleSubjectId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleSubjectId))
        {
            throw new BusinessException("Could not retrieve email or user information from Google.");
        }

        return new GoogleUserInfo(
            email,
            principal.FindFirstValue(ClaimTypes.Name),
            googleSubjectId,
            principal.FindFirst("picture")?.Value);
    }

    public string BuildTokenRedirectUrl(string redirectUri, TokenResponse token)
    {
        var uriBuilder = new UriBuilder(redirectUri);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["access_token"] = token.AccessToken;
        query["access_token_expiration"] = token.AccessTokenExpiration.ToString("o");
        query["refresh_token"] = token.RefreshToken;
        query["refresh_token_expiration"] = token.RefreshTokenExpiration.ToString("o");
        uriBuilder.Query = query.ToString();

        return uriBuilder.ToString();
    }
}
