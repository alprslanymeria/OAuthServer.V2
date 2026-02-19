using OAuthServer.V2.Core.Models;

namespace OAuthServer.V2.Core.Repositories;

// ENTITY-SPECIFIC REPOSITORY FOR PASSKEY CREDENTIALS.
// ENCAPSULATES ALL DATA ACCESS LOGIC SO THAT THE SERVICE LAYER DOES NOT DEPEND ON EF CORE.

public interface IPasskeyCredentialRepository
{
    Task<List<UserPasskeyCredential>> GetByUserIdAsync(string userId);
    Task<bool> ExistsByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default);
    Task<UserPasskeyCredential?> GetByCredentialIdAsync(byte[] credentialId);
    Task<List<UserPasskeyCredential>> GetByUserHandleAsync(byte[] userHandle, CancellationToken cancellationToken = default);
    Task AddAsync(UserPasskeyCredential credential);
    void Update(UserPasskeyCredential credential);
}
