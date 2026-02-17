using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OAuthServer.V2.Core.Common;
using OAuthServer.V2.Core.Configuration;
using OAuthServer.V2.Core.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace OAuthServer.V2.Infrastructure.Notifications;

public class SmsNotificationSender(

    IOptions<TwilioOption> options,
    ILogger<SmsNotificationSender> logger) : INotificationSender
{
    private readonly TwilioOption _twilioOption = options.Value;
    private readonly ILogger<SmsNotificationSender> _logger = logger;

    public DeliveryMethod Method => DeliveryMethod.Sms;

    public async Task SendAsync(string recipient, string subject, string body)
    {
        TwilioClient.Init(_twilioOption.AccountSid, _twilioOption.AuthToken);

        var messageResource = await MessageResource.CreateAsync(
            to: new PhoneNumber(recipient),
            from: new PhoneNumber(_twilioOption.FromPhoneNumber),
            body: body);

        _logger.LogInformation("SmsNotificationSender -> SMS SENT SUCCESSFULLY TO {Recipient}, SID: {Sid}",
            recipient, messageResource.Sid);
    }
}
