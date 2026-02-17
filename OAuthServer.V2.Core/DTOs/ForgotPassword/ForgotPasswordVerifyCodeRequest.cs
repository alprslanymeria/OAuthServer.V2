using OAuthServer.V2.Core.Common;

namespace OAuthServer.V2.Core.DTOs.ForgotPassword;

// IDENTIFIER --> PHONE NUMBER | E-MAIL | USERNAME
public record ForgotPasswordVerifyCodeRequest(

    string Identifier,
    string Code,
    DeliveryMethod DeliveryMethod);
