using OAuthServer.V2.Core.Common;
using OAuthServer.V2.Core.Exceptions;
using OAuthServer.V2.Core.Services;

namespace OAuthServer.V2.Infrastructure.Notifications;

public class NotificationService(IEnumerable<INotificationSender> senders) : INotificationService
{
    private readonly Dictionary<DeliveryMethod, INotificationSender> _senders = senders.ToDictionary(s => s.Method);

    public async Task SendAsync(DeliveryMethod method, string recipient, string subject, string body)
    {
        if (!_senders.TryGetValue(method, out var sender))
        {
            throw new BusinessException($"Unsupported delivery method: {method}");
        }

        await sender.SendAsync(recipient, subject, body);
    }
}
