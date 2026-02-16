using OAuthServer.V2.Core.Common;
using System.Linq.Expressions;

namespace OAuthServer.V2.Core.Services;

// THE METHODS IN THIS INTERFACE ARE IMPLEMENTED IN THE SERVICE LAYER.
// THE METHODS IN THIS INTERFACE CAN BE USED IN THE SERVICE OR PRESENTATION (API) LAYER.
// THE DATA RETURNED FROM THE METHODS CAN BE USED IN THE SERVICE OR PRESENTATION (API) LAYER.

public interface IServiceGeneric<TEntity, TDto> where TEntity : class where TDto : class
{
    Task<ServiceResult<IEnumerable<TDto>>> GetAllAsync();

    Task<ServiceResult<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate);

    ValueTask<ServiceResult<TDto>> GetByIdAsync(int id);

    Task<ServiceResult<TDto>> AddAsync(TDto dto);

    Task<ServiceResult> Update(TDto dto, int id);

    Task<ServiceResult> Delete(int id);
}