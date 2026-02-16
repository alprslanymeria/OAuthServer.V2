using OAuthServer.V2.Core.UnitOfWork;

namespace OAuthServer.V2.Data.UnitOfWork;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext _context = context;

    public Task<int> CommitAsync() => _context.SaveChangesAsync();
}