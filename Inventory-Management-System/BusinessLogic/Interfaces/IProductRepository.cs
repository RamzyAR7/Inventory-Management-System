using Inventory_Management_System.Entities;
using System.Linq.Expressions;

namespace Inventory_Management_System.BusinessLogic.Interfaces
{
    public interface IProductRepository: IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllAsyncWithNestedIncludes();
        Task<Product> GetAsyncWithNestedIncludesBy(Expression<Func<Product, bool>> predicate);
        Task<Product?> FindAsyncWithNestedIncludes(Expression<Func<Product, bool>> predicate);
        Task<IEnumerable<Product>> GetAllForWarehousesAsync(List<Guid> warehouseIds);
    }

}
