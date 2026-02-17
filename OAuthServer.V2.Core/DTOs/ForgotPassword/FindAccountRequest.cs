namespace OAuthServer.V2.Core.DTOs.ForgotPassword;

// IDENTIFIER --> PHONE NUMBER | E-MAIL | USERNAME
public record FindAccountRequest(string Identifier);
