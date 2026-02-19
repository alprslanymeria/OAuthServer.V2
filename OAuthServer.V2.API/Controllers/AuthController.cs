using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OAuthServer.V2.Core.DTOs.Client;
using OAuthServer.V2.Core.DTOs.RefreshToken;
using OAuthServer.V2.Core.DTOs.User;
using OAuthServer.V2.Core.Exceptions;
using OAuthServer.V2.Core.Services;

namespace OAuthServer.V2.API.Controllers;


// "api/[controller]"           => MATCHING BY HTTP METHOD TYPE        => USE IN SIMPLE STRUCTURES.
// "api/[controller]/[action]"  => ACTION METHOD NAME MATCHING         => USE IN COMPLEX STRUCTURES.


[Route("api/[controller]/[action]")]
[ApiController]
public class AuthController(

    Core.Services.IAuthenticationService authenticationService,
    IGoogleAuthService googleAuthService
    
    ) : BaseController
{
    private readonly Core.Services.IAuthenticationService _authenticationService = authenticationService;
    private readonly IGoogleAuthService _googleAuthService = googleAuthService;

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
        // BUSINESS VALIDATION DELEGATED TO SERVICE - THROWS ON INVALID
        _googleAuthService.ValidateRedirectUri(redirect_uri);

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback))
        };

        properties.Items["redirect_uri"] = redirect_uri;

        return Challenge(properties, "Google");
    }

    [HttpGet]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? error = null)
    {
        if (!string.IsNullOrEmpty(error))
        {
            throw new BusinessException($"Google authentication error: {error}");
        }

        // ASP.NET CORE SPECIFIC OPERATIONS - MUST STAY IN CONTROLLER
        var result = await HttpContext.AuthenticateAsync("ExternalCookie");

        if (!result.Succeeded || result.Principal is null)
        {
            throw new UnauthorizedException("External authentication failed.");
        }

        // EXTRACT USER INFO - BUSINESS LOGIC IN SERVICE
        var userInfo = _googleAuthService.ExtractUserInfo(result.Principal);

        // CLEAN UP EXTERNAL COOKIE
        await HttpContext.SignOutAsync("ExternalCookie");

        // CREATE TOKEN - BUSINESS LOGIC IN SERVICE
        var tokenResponse = await _authenticationService.CreateTokenByExternalLogin(
            userInfo.Email, userInfo.Name, userInfo.GoogleSubjectId, userInfo.Picture);

        // REDIRECT WITH TOKENS IF REDIRECT URI EXISTS
        if (result.Properties?.Items.TryGetValue("redirect_uri", out var redirectUri) == true && !string.IsNullOrEmpty(redirectUri))
        {
            return Redirect(_googleAuthService.BuildTokenRedirectUrl(redirectUri, tokenResponse.Data!));
        }

        return ActionResultInstance(tokenResponse);
    }
}
