using Microsoft.Extensions.DependencyInjection;
using OAuthServer.V2.Core.Services;
using OAuthServer.V2.Infrastructure.Notifications;

namespace OAuthServer.V2.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // NOTIFICATION STRATEGY PATTERN - REGISTER ALL SENDERS
        services.AddScoped<INotificationSender, EmailNotificationSender>();
        services.AddScoped<INotificationSender, SmsNotificationSender>();

        // NOTIFICATION SERVICE - RESOLVES CORRECT SENDER BY DELIVERY METHOD
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}
