using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OAuthServer.V2.Core.Configuration.Storage;
using OAuthServer.V2.Core.Services;
using OAuthServer.V2.Core.Services.Storage;
using OAuthServer.V2.Infrastructure.Cache;
using OAuthServer.V2.Infrastructure.Notifications;
using OAuthServer.V2.Infrastructure.OpenTelemetry;
using OAuthServer.V2.Infrastructure.Storage;

namespace OAuthServer.V2.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddNotificationServices()
            .AddCacheServices()
            .AddOpenTelemetryServicesExt(configuration)
            .AddStorageServices(configuration);

        return services;
    }

    private static IServiceCollection AddCacheServices(this IServiceCollection services)
    {
        services.AddScoped<ICacheService, CacheService>();

        return services;
    }

    private static IServiceCollection AddNotificationServices(this IServiceCollection services)
    {
        // NOTIFICATION STRATEGY PATTERN - REGISTER ALL SENDERS
        services.AddScoped<INotificationSender, EmailNotificationSender>();
        services.AddScoped<INotificationSender, SmsNotificationSender>();

        // NOTIFICATION SERVICE - RESOLVES CORRECT SENDER BY DELIVERY METHOD
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }

    private static IServiceCollection AddStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        // LOAD STORAGE CONFIGURATION AND VALIDATE
        var storageConfig = configuration
            .GetRequiredSection(StorageOption.Key)
            .Get<StorageOption>()
            ?? throw new InvalidOperationException("StorageConfig is missing in appsettings");

        // CONFIGURATION BINDINGS
        services.Configure<StorageOption>(configuration.GetSection(StorageOption.Key));
        services.Configure<GoogleCloudStorageOption>(configuration.GetSection(GoogleCloudStorageOption.Key));

        // COMMON STORAGE SERVICES
        services.AddScoped<IStorageService, StorageService>();

        // REGISTRATION BASED ON STORAGE TYPE
        switch (storageConfig.StorageType)
        {
            case StorageType.GoogleCloud:
                services.AddScoped<IStorageProvider, GoogleCloudStorageProvider>();
                break;

            default:
                throw new NotSupportedException($"Storage type '{storageConfig.StorageType}' is not supported.");
        }

        return services;
    }
}
