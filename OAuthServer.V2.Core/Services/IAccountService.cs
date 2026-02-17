using OAuthServer.V2.Core.Common;
using OAuthServer.V2.Core.DTOs.Account;
using OAuthServer.V2.Core.DTOs.ForgotPassword;

namespace OAuthServer.V2.Core.Services;

public interface IAccountService
{
    // THE METHODS IN THIS INTERFACE ARE IMPLEMENTED IN THE SERVICE LAYER.
    // THE METHODS IN THIS INTERFACE CAN BE USED IN THE SERVICE OR PRESENTATION (API) LAYER.
    // THE DATA RETURNED FROM THE METHODS CAN BE USED IN THE SERVICE OR PRESENTATION (API) LAYER.

    // FORGOT PASSWORD FLOW
    Task<ServiceResult<FindAccountResponse>> FindAccountAsync(FindAccountRequest request);
    Task<ServiceResult> SendForgotPasswordCodeAsync(ForgotPasswordSendCodeRequest request);
    Task<ServiceResult<ForgotPasswordVerifyCodeResponse>> VerifyForgotPasswordCodeAsync(ForgotPasswordVerifyCodeRequest request);
    Task<ServiceResult> ResetPasswordAsync(ResetPasswordRequest request);

    // CHANGE PASSWORD
    Task<ServiceResult> ChangePasswordAsync(string userId, ChangePasswordRequest request);

    // CHANGE PHONE NUMBER
    Task<ServiceResult> InitiatePhoneNumberChangeAsync(string userId, ChangePhoneNumberRequest request);
    Task<ServiceResult> VerifyPhoneNumberChangeAsync(string userId, VerifyChangePhoneNumberRequest request);

    // PROFILE
    Task<ServiceResult<UserProfileResponse>> GetProfileAsync(string userId);
    Task<ServiceResult<UserProfileResponse>> UpdateProfileAsync(string userId, UpdateProfileRequest request);

    // ACCOUNT MANAGEMENT
    Task<ServiceResult> DeactivateAccountAsync(string userId, DeactivateAccountRequest request);
    Task<ServiceResult> DeleteAccountAsync(string userId, DeleteAccountRequest request);
}
