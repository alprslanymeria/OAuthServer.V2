namespace OAuthServer.V2.Core.DTOs.ForgotPassword;

// IDENTIFIER --> PHONE NUMBER | E-MAIL | USERNAME
public record ResetPasswordRequest(

    string Identifier,
    string ResetToken,
    string NewPassword,
    string ConfirmPassword);
