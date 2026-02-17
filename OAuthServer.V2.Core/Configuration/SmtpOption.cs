namespace OAuthServer.V2.Core.Configuration;

public class SmtpOption
{
    public const string Key = "Smtp";
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FromEmail { get; set; } = default!;
    public string FromName { get; set; } = default!;
    public bool UseSsl { get; set; } = true;
}
