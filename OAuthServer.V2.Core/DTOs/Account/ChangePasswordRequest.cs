namespace OAuthServer.V2.Core.DTOs.Account;

public record ChangePasswordRequest(

    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);
