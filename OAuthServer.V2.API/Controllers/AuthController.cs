using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OAuthServer.V2.Core.DTOs.Client;
using OAuthServer.V2.Core.DTOs.RefreshToken;
using OAuthServer.V2.Core.DTOs.User;
using System.Security.Claims;
using System.Web;

namespace OAuthServer.V2.API.Controllers;


// "api/[controller]"           => MATCHING BY HTTP METHOD TYPE        => USE IN SIMPLE STRUCTURES.
// "api/[controller]/[action]"  => ACTION METHOD NAME MATCHING         => USE IN COMPLEX STRUCTURES.


[Route("api/[controller]")]
[ApiController]
public class AuthController(

    Core.Services.IAuthenticationService authenticationService,
    IConfiguration configuration) : BaseController
{
    private readonly Core.Services.IAuthenticationService _authenticationService = authenticationService;
    private readonly IConfiguration _configuration = configuration;

    [HttpPost]
    public async Task<IActionResult> CreateToken(SignInRequest request)
        => ActionResultInstance(await _authenticationService.CreateTokenAsync(request));

    [HttpPost]
    public async Task<IActionResult> RevokeRefreshToken(RefreshTokenRequest request)
        => ActionResultInstance(await _authenticationService.RevokeRefreshToken(request.Token));

    [HttpPost]
    public async Task<IActionResult> CreateTokenByRefreshToken(RefreshTokenRequest request)
        => ActionResultInstance(await _authenticationService.CreateTokenByRefreshToken(request.Token));

    [HttpPost]
    public async Task<IActionResult> CreateTokenByClient(ClientSignInRequest request)
        => ActionResultInstance(await _authenticationService.CreateTokenByClient(request));

    [HttpGet]
    public IActionResult GoogleLogin([FromQuery] string redirect_uri)
    {
        if (string.IsNullOrWhiteSpace(redirect_uri))
        {
            return BadRequest("redirect_uri is required.");
        }

        var allowedUris = _configuration.GetSection("AllowedRedirectUris").Get<string[]>() ?? [];
        var isAllowed = allowedUris.Any(allowed =>
            redirect_uri.StartsWith(allowed, StringComparison.OrdinalIgnoreCase));

        if (!isAllowed)
        {
            return BadRequest("Invalid redirect_uri.");
        }

        // DEFINE REDIRECT URI
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback))
        };

        properties.Items["redirect_uri"] = redirect_uri;

        return Challenge(properties, "Google");
    }

    [HttpGet]
    public async Task<IActionResult> GoogleCallback()
    {
        // READ GOOGLE USER INFO FROM EXTERNAL COOKIE
        var result = await HttpContext.AuthenticateAsync("ExternalCookie");

        if (!result.Succeeded || result.Principal is null)
        {
            return Unauthorized();
        }

        // PULL USER INFO FROM CLAIMS
        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        //var name = result.Principal.FindFirstValue(ClaimTypes.Name);
        var name = "Test";
        var googleSubjectId = result.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var picture = result.Principal.FindFirst("picture")?.Value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleSubjectId))
        {
            return BadRequest("Google'dan email veya kullanıcı bilgisi alınamadı.");
        }

        // DELETE THE TEMPORARY EXTERNAL COOKIE
        await HttpContext.SignOutAsync("ExternalCookie");

        // CREATE TOKEN
        var tokenResponse = await _authenticationService.CreateTokenByExternalLogin(email, name, googleSubjectId, picture);

        if (tokenResponse.IsFail)
        {
            return ActionResultInstance(tokenResponse);
        }

        // REDIRECT BACK TO CLIENT APPLICATION WITH TOKENS
        if (result.Properties?.Items.TryGetValue("redirect_uri", out var redirectUri) == true && !string.IsNullOrEmpty(redirectUri))
        {
            var token = tokenResponse.Data!;
            var uriBuilder = new UriBuilder(redirectUri);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["access_token"] = token.AccessToken;
            query["access_token_expiration"] = token.AccessTokenExpiration.ToString("o");
            query["refresh_token"] = token.RefreshToken;
            query["refresh_token_expiration"] = token.RefreshTokenExpiration.ToString("o");
            uriBuilder.Query = query.ToString();

            return Redirect(uriBuilder.ToString());
        }

        return ActionResultInstance(tokenResponse);
    }

}
