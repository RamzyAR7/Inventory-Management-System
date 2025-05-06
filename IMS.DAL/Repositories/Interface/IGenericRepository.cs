using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace IMS.DAL.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        //IQueryable<T> GetQueryable(); // Add this method
        //Task<bool> ExistsAsync(Guid id);

        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
        Task<T?> GetByExpressionAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<T?> GetByIdAsync(Guid id, params Expression<Func<T, object>>[] includes);
        Task<T?> GetByCompositeKeyAsync(Guid key1, Guid key2, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize,
            Expression<Func<T, bool>> predicate = null, params Expression<Func<T, object>>[] includeProperties);
        // CRUD
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task DeleteAsync(Guid key1, Guid key2);
    }
}
