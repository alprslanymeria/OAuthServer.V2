namespace OAuthServer.V2.Core.Services.Storage;

/// <summary>
/// ABSTRACTION FOR RETRIEVING SECRETS FROM A SECRET MANAGEMENT SERVICE
/// </summary>
public interface ISecretProvider
{
    Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);
}
