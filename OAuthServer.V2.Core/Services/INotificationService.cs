using OAuthServer.V2.Core.Common;

namespace OAuthServer.V2.Core.Services;

public interface INotificationService
{
    Task SendAsync(DeliveryMethod method, string recipient, string subject, string body);
}
