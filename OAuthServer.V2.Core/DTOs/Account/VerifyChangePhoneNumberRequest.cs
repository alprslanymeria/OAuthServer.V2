namespace OAuthServer.V2.Core.DTOs.Account;

public record VerifyChangePhoneNumberRequest(

    string NewPhoneNumber,
    string Code);
