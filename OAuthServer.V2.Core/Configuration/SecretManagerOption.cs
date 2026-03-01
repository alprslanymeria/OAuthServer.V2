namespace OAuthServer.V2.Core.Configuration;

/// <summary>
/// CONFIGURATION FOR GOOGLE CLOUD SECRET MANAGER
/// </summary>
public class SecretManagerOption
{
    public const string Key = "GoogleSecretManager";

    /// <summary>
    /// GOOGLE CLOUD PROJECT ID
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// NAME OF THE SECRET CONTAINING THE SERVICE ACCOUNT JSON KEY
    /// </summary>
    public string ServiceAccountSecretName { get; set; } = string.Empty;

    /// <summary>
    /// VERSION OF THE SECRET TO ACCESS (DEFAULT: "latest")
    /// </summary>
    public string SecretVersion { get; set; } = "latest";

    /// <summary>
    /// BASE64 ENCODED GCP SERVICE ACCOUNT KEY JSON.
    /// PREFERRED FOR DOCKER ENVIRONMENTS — ELIMINATES FILE DEPENDENCY.
    /// SET VIA ENVIRONMENT VARIABLE: GoogleSecretManager__CredentialBase64
    /// </summary>
    public string CredentialBase64 { get; set; } = string.Empty;

    /// <summary>
    /// PATH TO THE GCP SERVICE ACCOUNT KEY FILE.
    /// FALLBACK WHEN CredentialBase64 IS NOT SET.
    /// </summary>
    public string CredentialFilePath { get; set; } = string.Empty;
}
