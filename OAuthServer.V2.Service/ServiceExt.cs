using FluentValidation;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using OAuthServer.V2.Core.Services;
using OAuthServer.V2.Service.Services;

namespace OAuthServer.V2.Service;

public static class ServiceExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticationService, AuthenticationService >();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IFileStorageHelper, FileStorageHelper>();
        services.AddScoped(typeof(IServiceGeneric<,>), typeof(ServiceGeneric<,>));

        // MAPSTER
        services.AddMapster();

        // FLUENT VALIDATION
        services.AddValidatorsFromAssembly(typeof(ServiceAssembly).Assembly);

        return services;
    }
}