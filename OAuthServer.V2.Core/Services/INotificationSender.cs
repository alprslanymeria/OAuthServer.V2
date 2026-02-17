using OAuthServer.V2.Core.Common;

namespace OAuthServer.V2.Core.Services;

public interface INotificationSender
{
    DeliveryMethod Method { get; }
    Task SendAsync(string recipient, string subject, string body);
}
