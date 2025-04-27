using Inventory_Management_System.Entities;
using System.Linq.Expressions;

namespace Inventory_Management_System.BusinessLogic.Interfaces
{
    public interface IWarehouseStockRepository : IGenericRepository<WarehouseStock>
    {
        Task DeleteAsync(Guid warehouseId, Guid productId);
        Task<WarehouseStock?> GetByCompositeKeyAsync(Guid warehouseId, Guid productId);
        Task UpdateAsync(WarehouseStock entity);
        Task AddAsync(WarehouseStock entity);
        IQueryable<WarehouseStock> Find(Expression<Func<WarehouseStock, bool>> predicate);
    }
}
