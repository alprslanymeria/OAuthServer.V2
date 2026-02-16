namespace OAuthServer.V2.Core.Configuration.Storage;

/// <summary>
/// MAIN STORAGE CONFIGURATION THAT DETERMINES WHICH PROVIDER TO USE
/// </summary>
public class StorageOption
{
    public const string Key = "Storage";

    /// <summary>
    /// THE TYPE OF STORAGE PROVIDER TO USE
    /// </summary>
    public StorageType StorageType { get; set; } = StorageType.GoogleCloud;

    /// <summary>
    /// WHETHER TO ENABLE STORAGE SERVICE
    /// </summary>
    public bool Enable { get; set; } = true;
}