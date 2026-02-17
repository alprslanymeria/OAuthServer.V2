namespace OAuthServer.V2.Core.DTOs.ForgotPassword;

// alpar****@gmail.com
// 53**76*****03
public record FindAccountResponse(

    string? MaskedEmail,
    string? MaskedPhone);
