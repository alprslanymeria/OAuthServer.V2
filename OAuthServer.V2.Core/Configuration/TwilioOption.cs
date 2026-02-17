namespace OAuthServer.V2.Core.Configuration;

public class TwilioOption
{
    public const string Key = "Twilio";
    public string AccountSid { get; set; } = default!;
    public string AuthToken { get; set; } = default!;
    public string FromPhoneNumber { get; set; } = default!;
}
