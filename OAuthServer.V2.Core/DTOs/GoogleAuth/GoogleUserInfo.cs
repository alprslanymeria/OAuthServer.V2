namespace OAuthServer.V2.Core.DTOs.GoogleAuth;

public record GoogleUserInfo(

    string Email,
    string? Name,
    string GoogleSubjectId,
    string? Picture);
