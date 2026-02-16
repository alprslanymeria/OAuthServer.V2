using System.Linq.Expressions;

namespace OAuthServer.V2.Core.Repositories;

// THE METHODS IN THIS INTERFACE ARE IMPLEMENTED IN THE DATA LAYER.
// THE METHODS IN THIS INTERFACE CAN BE USED IN THE DATA OR SERVICE LAYER.
// THE DATA RETURNED FROM THE METHODS CAN BE USED IN THE DATA OR SERVICE LAYER.

public interface IGenericRepository<TEntity> where TEntity : class
{

    // GENERAL CRUD OPERATIONS FOR ENTITIES WILL BE DEFINED HERE.
    // IN SMALL AND MEDIUM-SIZED PROJECTS, THE GENERIC REPOSITORY PATTERN IS SUFFICIENT. IN LARGER LEVELS, DDD IS USED.

    // UNTIL THE ToList() METHOD IS CALLED, IT DOES NOT REFLECT TO THE DATABASE.
    IQueryable<TEntity> GetAll();

    IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);

    ValueTask<TEntity?> GetByIdAsync(int id);

    ValueTask AddAsync(TEntity entity);

    TEntity Update(TEntity entity);

    void Delete(TEntity entity);
}