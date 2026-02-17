using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using OAuthServer.V2.Core.Common;
using OAuthServer.V2.Core.Configuration;
using OAuthServer.V2.Core.Services;

namespace OAuthServer.V2.Infrastructure.Notifications;

public class EmailNotificationSender(

    IOptions<SmtpOption> options,
    ILogger<EmailNotificationSender> logger) : INotificationSender
{
    private readonly SmtpOption _smtpOption = options.Value;
    private readonly ILogger<EmailNotificationSender> _logger = logger;

    public DeliveryMethod Method => DeliveryMethod.Email;

    public async Task SendAsync(string recipient, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpOption.FromName, _smtpOption.FromEmail));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();

        await client.ConnectAsync(_smtpOption.Host, _smtpOption.Port, _smtpOption.UseSsl);
        await client.AuthenticateAsync(_smtpOption.Username, _smtpOption.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("EmailNotificationSender -> EMAIL SENT SUCCESSFULLY TO {Recipient}", recipient);
    }
}
