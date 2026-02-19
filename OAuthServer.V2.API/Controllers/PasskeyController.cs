using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OAuthServer.V2.Core.DTOs.Passkeys;
using OAuthServer.V2.Core.Exceptions;
using OAuthServer.V2.Core.Services;

namespace OAuthServer.V2.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class PasskeyController(IPasskeyService passkeyService) : BaseController
{
    private readonly IPasskeyService _passkeyService = passkeyService;

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> RegisterBegin()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedException("User not authenticated.");

        return ActionResultInstance(await _passkeyService.RegisterBeginAsync(userId));
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> RegisterComplete(PasskeyRegisterCompleteRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedException("User not authenticated.");

        return ActionResultInstance(await _passkeyService.RegisterCompleteAsync(userId, request));
    }

    [HttpPost]
    public async Task<IActionResult> LoginBegin(PasskeyLoginBeginRequest request)
        => ActionResultInstance(await _passkeyService.LoginBeginAsync(request));

    [HttpPost]
    public async Task<IActionResult> LoginComplete(PasskeyLoginCompleteRequest request)
        => ActionResultInstance(await _passkeyService.LoginCompleteAsync(request));
}
