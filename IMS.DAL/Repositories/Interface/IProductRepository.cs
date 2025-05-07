using IMS.Domain.Entities;
using System.Linq.Expressions;

namespace IMS.DAL.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllAsyncWithNestedIncludes();
        Task<IEnumerable<Product>> GetAllForWarehousesAsync(List<Guid> warehouseIds);
        Task<Product> GetAsyncWithNestedIncludesBy(Expression<Func<Product, bool>> predicate);
        Task<Product?> FindAsyncWithNestedIncludes(Expression<Func<Product, bool>> predicate);
        Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsyncWithNestedIncludes(
            int pageNumber,
            int pageSize,
            Expression<Func<Product, bool>> predicate = null,
            Expression<Func<Product, object>> orderBy = null,
            bool sortDescending = false);
    }
}
