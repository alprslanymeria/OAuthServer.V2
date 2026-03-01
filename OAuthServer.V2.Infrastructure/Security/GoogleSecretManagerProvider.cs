using Google.Apis.Auth.OAuth2;
using Google.Cloud.SecretManager.V1;
using Grpc.Auth;
using Microsoft.Extensions.Options;
using OAuthServer.V2.Core.Configuration;
using OAuthServer.V2.Core.Services.Storage;
using System.Text;

namespace OAuthServer.V2.Infrastructure.Security;

/// <summary>
/// GOOGLE CLOUD SECRET MANAGER IMPLEMENTATION FOR RETRIEVING SECRETS
/// </summary>
public class GoogleSecretManagerProvider : ISecretProvider
{
    private readonly SecretManagerServiceClient _client;
    private readonly SecretManagerOption _config;

    #region UTILS
    /// <summary>
    /// RESOLVES GCP CREDENTIAL USING CredentialFactory
    /// </summary>
    private static GoogleCredential ResolveCredential(SecretManagerOption config)
    {
        // PRIORITY 1: BASE64 ENCODED CREDENTIAL JSON
        if (!string.IsNullOrWhiteSpace(config.CredentialBase64))
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(config.CredentialBase64));
            return CredentialFactory.FromJson<ServiceAccountCredential>(json).ToGoogleCredential();
        }

        // PRIORITY 2: CREDENTIAL FILE PATH
        if (!string.IsNullOrWhiteSpace(config.CredentialFilePath))
        {
            return CredentialFactory.FromFile<ServiceAccountCredential>(config.CredentialFilePath).ToGoogleCredential();
        }

        throw new InvalidOperationException(
            "No GCP credential source configured. " +
            "Set 'GoogleSecretManager:CredentialBase64' (base64 encoded JSON — recommended for Docker) " +
            "or 'GoogleSecretManager:CredentialFilePath' (file path — for local development).");
    }
    #endregion

    public GoogleSecretManagerProvider(IOptions<SecretManagerOption> config)
    {
        _config = config.Value;

        var credential = ResolveCredential(_config)
            .CreateScoped(SecretManagerServiceClient.DefaultScopes);

        _client = new SecretManagerServiceClientBuilder
        {
            ChannelCredentials = credential.ToChannelCredentials()
        }.Build();
    }

    public async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        var secretVersionName = new SecretVersionName(_config.ProjectId, secretName, _config.SecretVersion);
        var result = await _client.AccessSecretVersionAsync(secretVersionName, cancellationToken);
        return result.Payload.Data.ToStringUtf8();
    }
}
