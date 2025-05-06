using IMS.DAL.Entities;
using System.Linq.Expressions;

namespace IMS.DAL.Repositories.Interfaces
{
    public interface IProductRepository: IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllAsyncWithNestedIncludes();
        Task<Product> GetAsyncWithNestedIncludesBy(Expression<Func<Product, bool>> predicate);
        Task<Product?> FindAsyncWithNestedIncludes(Expression<Func<Product, bool>> predicate);
        Task<IEnumerable<Product>> GetAllForWarehousesAsync(List<Guid> warehouseIds);
        Task<Product> FindFirstOrDefaultAsync(Expression<Func<Product, bool>> predicate);
    }

}
