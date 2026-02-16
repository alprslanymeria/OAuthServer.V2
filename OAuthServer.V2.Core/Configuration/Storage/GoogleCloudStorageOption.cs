namespace OAuthServer.V2.Core.Configuration.Storage;

/// <summary>
/// CONFIGURATION FOR GOOGLE CLOUD STORAGE
/// </summary>
public class GoogleCloudStorageOption
{
    public const string Key = "GoogleCloudStorage";

    /// <summary>
    /// GOOGLE CLOUD STORAGE BUCKET NAME
    /// </summary>
    public string BucketName { get; set; } = string.Empty;
}