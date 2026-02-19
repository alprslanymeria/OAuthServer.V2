using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OAuthServer.V2.Core.Common;
using OAuthServer.V2.Core.DTOs.User;
using OAuthServer.V2.Core.DTOs.Verification;
using OAuthServer.V2.Core.Exceptions;
using OAuthServer.V2.Core.Models;
using OAuthServer.V2.Core.Services;

namespace OAuthServer.V2.Service.Services;

public class UserService(

    UserManager<User> userManager,
    INotificationService notificationService,
    IFileStorageHelper fileStorageHelper,
    IMapper mapper,
    ILogger<UserService> logger

    ) : IUserService
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IFileStorageHelper _fileStorageHelper = fileStorageHelper;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<UserService> _logger = logger;

    public async Task<ServiceResult<UserDto>> CreateUserAsync(SignUpRequest request)
    {
        // CHECK PHONE NUMBER UNIQUENESS IF PROVIDED
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var existingUser = await _userManager.Users.Where(u => u.PhoneNumber == request.PhoneNumber).SingleOrDefaultAsync();

            if (existingUser is not null)
            {
                throw new ConflictException("Phone number is already in use.");
            }
        }

        // IN HERE, WE DID MANUAL MAPPING BECAUSE THE REQUEST CONTAINS IMPORTANT INFORMATION.
        var user = new User
        {
            FirstName = request.FirstName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            BirthDate = request.BirthDate,
            UserName = request.UserName ?? request.Email?.Split('@')[0] ?? $"user_{Guid.NewGuid():N}"[..20]
        };

        // CREATE USER WITH USER MANAGER
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            throw new BusinessException(result.Errors.Select(e => e.Description).ToList());
        }

        // UPLOAD USER IMAGE TO STORAGE IF PROVIDED
        if (request.Image is not null)
        {
        
            user.Image = await _fileStorageHelper.UploadFileToStorageAsync(request.Image, user.Id, "profile");
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("UserService -> IMAGE UPLOADED FOR USER {UserId}: {ImageUrl}", user.Id, user.Image);
        }

        // SEND VERIFICATION CODE AUTOMATICALLY AFTER SIGNUP
        await SendVerificationCodeInternalAsync(user, request.Email, request.PhoneNumber);

        // USER MAP TO USERDTO
        var userDto = _mapper.Map<UserDto>(user);

        return ServiceResult<UserDto>.SuccessAsCreated(userDto, $"api/user/{userDto.UserName}");
    }

    public async Task<ServiceResult> SendVerificationCodeAsync(SendVerificationCodeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (user, method) = await FindUserForVerificationAsync(request.Email, request.PhoneNumber);

        // CHECK IF ALREADY VERIFIED
        if (method == DeliveryMethod.Email && user.EmailConfirmed)
        {
            throw new ConflictException("Email is already verified.");
        }

        if (method == DeliveryMethod.Sms && user.PhoneNumberConfirmed)
        {
            throw new ConflictException("Phone number is already verified.");
        }

        await SendVerificationCodeInternalAsync(user, request.Email, request.PhoneNumber);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> VerifySignUpAsync(VerifySignUpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (user, method) = await FindUserForVerificationAsync(request.Email, request.PhoneNumber);

        // VERIFY CODE AND UPDATE CONFIRMATION STATUS
        var provider = method == DeliveryMethod.Email
            ? TokenOptions.DefaultEmailProvider
            : TokenOptions.DefaultPhoneProvider;

        var isValid = await _userManager.VerifyUserTokenAsync(user, provider, "SignUpVerification", request.Code);

        if (!isValid)
        {
            throw new BusinessException("Invalid or expired verification code.");
        }

        if (method == DeliveryMethod.Email)
            user.EmailConfirmed = true;
        else
            user.PhoneNumberConfirmed = true;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            throw new BusinessException(updateResult.Errors.Select(e => e.Description).ToList());
        }

        _logger.LogInformation("UserService -> SIGNUP VERIFIED FOR USER {UserId}", user.Id);

        return ServiceResult.Success();
    }

    #region HELPERS

    private async Task<(User User, DeliveryMethod Method)> FindUserForVerificationAsync(string? email, string? phoneNumber)
    {
        User? user;
        DeliveryMethod method;

        if (!string.IsNullOrWhiteSpace(email))
        {
            user = await _userManager.FindByEmailAsync(email);
            method = DeliveryMethod.Email;
        }
        else if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            user = await _userManager.Users.Where(u => u.PhoneNumber == phoneNumber).SingleOrDefaultAsync();
            method = DeliveryMethod.Sms;
        }
        else
        {
            throw new BusinessException("Email or phone number is required.");
        }

        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        return (user, method);
    }

    private async Task SendVerificationCodeInternalAsync(User user, string? email, string? phoneNumber)
    {
        var (method, provider, recipient) = !string.IsNullOrWhiteSpace(email)
            ? (DeliveryMethod.Email, TokenOptions.DefaultEmailProvider, email!)
            : (DeliveryMethod.Sms, TokenOptions.DefaultPhoneProvider, phoneNumber!);

        var code = await _userManager.GenerateUserTokenAsync(user, provider, "SignUpVerification");

        await _notificationService.SendAsync(method, recipient, "Verify Your Account",$"Your verification code is: {code}");

        _logger.LogInformation("UserService -> VERIFICATION CODE SENT VIA {Method} FOR USER {UserId}", method, user.Id);
    }

    #endregion
}