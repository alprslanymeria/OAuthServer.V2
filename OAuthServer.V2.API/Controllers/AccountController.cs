using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OAuthServer.V2.Core.DTOs.Account;
using OAuthServer.V2.Core.DTOs.ForgotPassword;
using OAuthServer.V2.Core.Services;
using System.Security.Claims;

namespace OAuthServer.V2.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController(

    IAccountService accountService) : BaseController
{
    private readonly IAccountService _accountService = accountService;

    #region FORGOT PASSWORD FLOW

    [HttpPost("forgot-password")]
    public async Task<IActionResult> FindAccount(FindAccountRequest request)
        => ActionResultInstance(await _accountService.FindAccountAsync(request));

    [HttpPost("forgot-password/send-code")]
    public async Task<IActionResult> SendForgotPasswordCode(ForgotPasswordSendCodeRequest request)
        => ActionResultInstance(await _accountService.SendForgotPasswordCodeAsync(request));

    [HttpPost("forgot-password/verify-code")]
    public async Task<IActionResult> VerifyForgotPasswordCode(ForgotPasswordVerifyCodeRequest request)
        => ActionResultInstance(await _accountService.VerifyForgotPasswordCodeAsync(request));

    [HttpPost("forgot-password/reset")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        => ActionResultInstance(await _accountService.ResetPasswordAsync(request));

    #endregion

    #region CHANGE PASSWORD

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        return ActionResultInstance(await _accountService.ChangePasswordAsync(userId, request));
    }

    #endregion

    #region CHANGE PHONE NUMBER

    [Authorize]
    [HttpPost("change-phone")]
    public async Task<IActionResult> InitiatePhoneNumberChange(ChangePhoneNumberRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        return ActionResultInstance(await _accountService.InitiatePhoneNumberChangeAsync(userId, request));
    }

    [Authorize]
    [HttpPost("change-phone/verify")]
    public async Task<IActionResult> VerifyPhoneNumberChange(VerifyChangePhoneNumberRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        return ActionResultInstance(await _accountService.VerifyPhoneNumberChangeAsync(userId, request));
    }

    #endregion

    #region PROFILE

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        return ActionResultInstance(await _accountService.GetProfileAsync(userId));
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        return ActionResultInstance(await _accountService.UpdateProfileAsync(userId, request));
    }

    #endregion

    #region ACCOUNT MANAGEMENT

    [Authorize]
    [HttpPost("deactivate")]
    public async Task<IActionResult> DeactivateAccount(DeactivateAccountRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        return ActionResultInstance(await _accountService.DeactivateAccountAsync(userId, request));
    }

    [Authorize]
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteAccount(DeleteAccountRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        return ActionResultInstance(await _accountService.DeleteAccountAsync(userId, request));
    }

    #endregion

    #region HELPERS

    private string? GetUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier);

    #endregion
}
