using Mapster;
using OAuthServer.V2.Core.DTOs.User;
using OAuthServer.V2.Core.Models;

namespace OAuthServer.V2.Service.Mapping;

internal class MapperConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ENTITY TO DTO
        config.NewConfig<User, UserDto>();
    }
}