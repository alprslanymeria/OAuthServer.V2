using System.Text;
using System.Text.Json;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Mapster;
using OAuthServer.V2.Core.Models;

namespace OAuthServer.V2.Service.Mapping;

internal class PasskeyMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // USER → FIDO2 USER
        config.NewConfig<User, Fido2User>()
            .Map(dest => dest.Id, src => Encoding.UTF8.GetBytes(src.Id))
            .Map(dest => dest.Name, src => src.UserName ?? src.Email ?? src.Id)
            .Map(dest => dest.DisplayName, src => src.FirstName);

        // REGISTERED CREDENTIAL → ENTITY
        config.NewConfig<RegisteredPublicKeyCredential, UserPasskeyCredential>()
            .Map(dest => dest.CredentialId, src => src.Id)
            .Map(dest => dest.UserHandle, src => src.User.Id)
            .Map(dest => dest.Transports, src => src.Transports != null
                ? JsonSerializer.Serialize(src.Transports)
                : null)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.UserId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.User);

        // ENTITY → CREDENTIAL DESCRIPTOR (FOR EXCLUDE / ALLOW LISTS)
        config.NewConfig<UserPasskeyCredential, PublicKeyCredentialDescriptor>()
            .MapWith(src => new PublicKeyCredentialDescriptor(
                PublicKeyCredentialType.PublicKey,
                src.CredentialId,
                null));
    }
}
