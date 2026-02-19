using Microsoft.EntityFrameworkCore;
using OAuthServer.V2.Core.Models;
using OAuthServer.V2.Core.Repositories;

namespace OAuthServer.V2.Data.Repositories;

public class PasskeyCredentialRepository(AppDbContext context) : IPasskeyCredentialRepository
{
    private readonly DbSet<UserPasskeyCredential> _dbSet = context.Set<UserPasskeyCredential>();
    private readonly DbContext _context = context;

    public async Task<List<UserPasskeyCredential>> GetByUserIdAsync(string userId)
        => await _dbSet.Where(x => x.UserId == userId).AsNoTracking().ToListAsync();

    public async Task<bool> ExistsByCredentialIdAsync(byte[] credentialId, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(x => x.CredentialId == credentialId, cancellationToken);

    public async Task<UserPasskeyCredential?> GetByCredentialIdAsync(byte[] credentialId)
        => await _dbSet.Where(x => x.CredentialId == credentialId).AsNoTracking().SingleOrDefaultAsync();

    public async Task<List<UserPasskeyCredential>> GetByUserHandleAsync(byte[] userHandle, CancellationToken cancellationToken = default)
        => await _dbSet.Where(x => x.UserHandle == userHandle).AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(UserPasskeyCredential credential)
        => await _dbSet.AddAsync(credential);

    public void Update(UserPasskeyCredential credential)
        => _context.Entry(credential).State = EntityState.Modified;
}
