using Microsoft.EntityFrameworkCore;
using OAuthServer.V2.Core.Repositories;
using System.Linq.Expressions;

namespace OAuthServer.V2.Data.Repositories;

public class GenericRepository<TEntity>(AppDbContext context) : IGenericRepository<TEntity> where TEntity : class
{
    private readonly DbContext _context = context;
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();


    // OPERATIONS THAT IN THESE METHODS ARE NOT REFLECTED TO THE DATABASE, WHEN SaveChangesAsync() IS CALLED IN THE SERVICE LAYER, THEN IT WILL BE REFLECTED TO THE DATABASE.

    public IQueryable<TEntity> GetAll() => _dbSet.AsQueryable().AsNoTracking();

    // WHEN ToListAsync() IS CALLED IN THE SERVICE LAYER, THEN IT WILL BE REFLECTED TO THE DATABASE.
    public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate) => _dbSet.Where(predicate).AsNoTracking();

    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate)
        => await _dbSet.Where(predicate).AsNoTracking().SingleOrDefaultAsync();

    public async ValueTask<TEntity?> GetByIdAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);

        if (entity is null) return entity;

        _context.Entry(entity).State = EntityState.Detached;

        return entity;
    }

    public async ValueTask AddAsync(TEntity entity) => await _dbSet.AddAsync(entity);

    public TEntity Update(TEntity entity)
    {
        //| Yöntem             | Kısa Açıklama                           | Tavsiye             |
        //| ------------------ | --------------------------------------- | ------------------  |
        //| `Update()`         | Çok agresif, tüm graph'ı günceller      | ❌ Kaçın            |
        //| `State = Modified` | Kontrollü ama tüm kolonları update eder | ⚠️ Bazı durumlarda  |
        //| `SetValues()`      | En güvenli, en temiz update yöntemi     | ✅ Şiddetle tavsiye |

        _context.Entry(entity).State = EntityState.Modified;
        return entity;
    }

    public void Delete(TEntity entity) => _dbSet.Remove(entity);
}