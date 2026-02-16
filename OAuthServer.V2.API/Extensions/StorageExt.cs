using OAuthServer.V2.Core.Configuration.Storage;
using OAuthServer.V2.Core.Services.Storage;
using OAuthServer.V2.Service.Services.Storage;

namespace OAuthServer.V2.API.Extensions;

public static class StorageExt
{
    public static IServiceCollection AddStorageServicesExt(this IServiceCollection services, IConfiguration configuration)
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
                AddGoogleCloudStorage(services);
                break;

            default:
                throw new NotSupportedException($"Storage type '{storageConfig.StorageType}' is not supported.");
        }

        return services;
    }

    private static void AddGoogleCloudStorage(IServiceCollection services)
    {
        services.AddScoped<IStorageProvider, GoogleCloudStorageProvider>();
    }
}
