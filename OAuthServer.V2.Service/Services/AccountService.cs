using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OAuthServer.V2.Core.Common;
using OAuthServer.V2.Core.DTOs.Account;
using OAuthServer.V2.Core.DTOs.ForgotPassword;
using OAuthServer.V2.Core.Exceptions;
using OAuthServer.V2.Core.Models;
using OAuthServer.V2.Core.Services;

namespace OAuthServer.V2.Service.Services;

public class AccountService(

    UserManager<User> userManager,
    INotificationService notificationService,
    IFileStorageHelper fileStorageHelper,
    ILogger<AccountService> logger) : IAccountService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IFileStorageHelper _fileStorageHelper = fileStorageHelper;
    private readonly ILogger<AccountService> _logger = logger;

    #region FORGOT PASSWORD FLOW

    public async Task<ServiceResult<FindAccountResponse>> FindAccountAsync(FindAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await FindUserByIdentifierAsync(request.Identifier);
        if (!user.IsActive) throw new ForbiddenException("Account is deactivated.");

        var response = new FindAccountResponse(
            MaskedEmail: user.Email is not null ? MaskEmail(user.Email) : null,
            MaskedPhone: user.PhoneNumber is not null ? MaskPhoneNumber(user.PhoneNumber) : null);

        return ServiceResult<FindAccountResponse>.Success(response);
    }

    public async Task<ServiceResult> SendForgotPasswordCodeAsync(ForgotPasswordSendCodeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await FindUserByIdentifierAsync(request.Identifier);

        var (provider, recipient) = ResolveNotificationTarget(user, request.DeliveryMethod);

        // GENERATE CODE AND SEND VIA NOTIFICATION SERVICE
        var code = await _userManager.GenerateUserTokenAsync(user, provider, "ForgotPassword");

        await _notificationService.SendAsync(request.DeliveryMethod, recipient, "Password Reset Code", $"Your password reset code is: {code}");

        _logger.LogInformation("AccountService -> FORGOT PASSWORD CODE SENT VIA {Method} FOR USER {UserId}", request.DeliveryMethod, user.Id);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<ForgotPasswordVerifyCodeResponse>> VerifyForgotPasswordCodeAsync(ForgotPasswordVerifyCodeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await FindUserByIdentifierAsync(request.Identifier);

        var provider = request.DeliveryMethod == DeliveryMethod.Email
            ? TokenOptions.DefaultEmailProvider
            : TokenOptions.DefaultPhoneProvider;

        var isValid = await _userManager.VerifyUserTokenAsync(user, provider, "ForgotPassword", request.Code);
        if (!isValid) throw new BusinessException("Invalid or expired verification code.");

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        _logger.LogInformation("AccountService -> FORGOT PASSWORD CODE VERIFIED FOR USER {UserId}", user.Id);

        return ServiceResult<ForgotPasswordVerifyCodeResponse>.Success(new ForgotPasswordVerifyCodeResponse(resetToken));
    }

    public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await FindUserByIdentifierAsync(request.Identifier);

        var result = await _userManager.ResetPasswordAsync(user, request.ResetToken, request.NewPassword);

        if (!result.Succeeded) throw new BusinessException(result.Errors.Select(e => e.Description).ToList());

        _logger.LogInformation("AccountService -> PASSWORD RESET SUCCESSFULLY FOR USER {UserId}", user.Id);

        return ServiceResult.Success();
    }

    #endregion

    #region CHANGE PASSWORD

    public async Task<ServiceResult> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await GetUserByIdAsync(userId);

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!result.Succeeded) throw new BusinessException(result.Errors.Select(e => e.Description).ToList());

        _logger.LogInformation("AccountService -> PASSWORD CHANGED SUCCESSFULLY FOR USER {UserId}", userId);

        return ServiceResult.Success();
    }

    #endregion

    #region CHANGE PHONE NUMBER

    public async Task<ServiceResult> InitiatePhoneNumberChangeAsync(string userId, ChangePhoneNumberRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await GetUserByIdAsync(userId);

        // CHECK IF PHONE NUMBER IS ALREADY IN USE BY ANOTHER USER
        var existingUser = await _userManager.Users.Where(u => u.PhoneNumber == request.NewPhoneNumber && u.Id != userId).SingleOrDefaultAsync();

        if (existingUser is not null) throw new ConflictException("This phone number is already in use.");

        // GENERATE CODE AND SEND VIA NOTIFICATION SERVICE
        var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, request.NewPhoneNumber);

        await _notificationService.SendAsync(DeliveryMethod.Sms, request.NewPhoneNumber, "Phone Verification", $"Your phone verification code is: {code}");

        _logger.LogInformation("AccountService -> PHONE CHANGE CODE SENT FOR USER {UserId}", userId);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> VerifyPhoneNumberChangeAsync(string userId, VerifyChangePhoneNumberRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await GetUserByIdAsync(userId);

        var result = await _userManager.ChangePhoneNumberAsync(user, request.NewPhoneNumber, request.Code);

        if (!result.Succeeded) throw new BusinessException(result.Errors.Select(e => e.Description).ToList());

        _logger.LogInformation("AccountService -> PHONE NUMBER CHANGED SUCCESSFULLY FOR USER {UserId}", userId);

        return ServiceResult.Success();
    }

    #endregion

    #region PROFILE

    public async Task<ServiceResult<UserProfileResponse>> GetProfileAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        return ServiceResult<UserProfileResponse>.Success(MapToProfileResponse(user));
    }

    public async Task<ServiceResult<UserProfileResponse>> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await GetUserByIdAsync(userId);

        // UPDATE ONLY PROVIDED FIELDS
        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.BirthDate.HasValue) user.BirthDate = request.BirthDate.Value;
        if (request.Locale is not null) user.Locale = request.Locale;

        // HANDLE IMAGE UPLOAD
        if (request.Image is not null)
        {
            // DELETE OLD IMAGE IF EXISTS
            if (!string.IsNullOrEmpty(user.Image))
            {
                await _fileStorageHelper.DeleteFileFromStorageAsync(user.Image);
            }

            user.Image = await _fileStorageHelper.UploadFileToStorageAsync(request.Image, userId, "profile");
        }

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded) throw new BusinessException(result.Errors.Select(e => e.Description).ToList());

        _logger.LogInformation("AccountService -> PROFILE UPDATED FOR USER {UserId}", userId);

        return ServiceResult<UserProfileResponse>.Success(MapToProfileResponse(user));
    }

    #endregion

    #region ACCOUNT MANAGEMENT

    public async Task<ServiceResult> DeactivateAccountAsync(string userId, DeactivateAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await GetUserByIdAsync(userId);
        await ValidatePasswordAsync(user, request.Password);

        user.IsActive = false;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded) throw new BusinessException(result.Errors.Select(e => e.Description).ToList());

        _logger.LogInformation("AccountService -> ACCOUNT DEACTIVATED FOR USER {UserId}", userId);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAccountAsync(string userId, DeleteAccountRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await GetUserByIdAsync(userId);
        await ValidatePasswordAsync(user, request.Password);

        // DELETE PROFILE IMAGE IF EXISTS
        if (!string.IsNullOrEmpty(user.Image))
        {
            await _fileStorageHelper.DeleteFileFromStorageAsync(user.Image);
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded) throw new BusinessException(result.Errors.Select(e => e.Description).ToList());

        _logger.LogInformation("AccountService -> ACCOUNT DELETED FOR USER {UserId}", userId);

        return ServiceResult.Success();
    }

    #endregion

    #region HELPERS

    private async Task<User> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User not found.");
    }

    private async Task<User> FindUserByIdentifierAsync(string identifier)
    {
        // TRY EMAIL
        var user = await _userManager.FindByEmailAsync(identifier);
        if (user is not null) return user;

        // TRY USERNAME
        user = await _userManager.FindByNameAsync(identifier);
        if (user is not null) return user;

        // TRY PHONE NUMBER
        user = await _userManager.Users.Where(u => u.PhoneNumber == identifier).SingleOrDefaultAsync();

        return user ?? throw new NotFoundException("Account not found.");
    }

    private async Task ValidatePasswordAsync(User user, string password)
    {
        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid) throw new UnauthorizedException("Invalid password.");
    }

    private static (string Provider, string Recipient) ResolveNotificationTarget(User user, DeliveryMethod method)
    {
        return method switch
        {
            DeliveryMethod.Email => (
                TokenOptions.DefaultEmailProvider,
                user.Email ?? throw new BusinessException("No email associated with this account.")),

            DeliveryMethod.Sms => (
                TokenOptions.DefaultPhoneProvider,
                user.PhoneNumber ?? throw new BusinessException("No phone number associated with this account.")),

            _ => throw new BusinessException($"Unsupported delivery method: {method}")
        };
    }

    private static string MaskEmail(string email)
    {
        var parts = email.Split('@');
        if (parts[0].Length <= 2) return parts[0][0] + "***@" + parts[1];

        return parts[0][0] + "***" + parts[0][^1] + "@" + parts[1];
    }

    private static string MaskPhoneNumber(string phone)
    {
        if (phone.Length <= 4) return "****";
        return new string('*', phone.Length - 4) + phone[^4..];
    }

    private static UserProfileResponse MapToProfileResponse(User user)
    {
        return new UserProfileResponse(
            user.UserName, user.Email, user.PhoneNumber, user.Image,
            user.FirstName, user.BirthDate, user.Locale,
            user.IsActive, user.EmailConfirmed, user.PhoneNumberConfirmed);
    }

    #endregion
}
