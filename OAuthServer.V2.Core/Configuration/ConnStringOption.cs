namespace OAuthServer.V2.Core.Configuration;

public class ConnStringOption
{
    public const string Key = "ConnectionStrings";
    public string SqlServer { get; set; } = default!;
}