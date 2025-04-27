using Inventory_Management_System.Entities;

namespace Inventory_Management_System.BusinessLogic.Interfaces
{
    public interface IWarehouseStockRepository : IGenericRepository<WarehouseStock>
    {
        Task DeleteAsync(Guid warehouseId, Guid productId);
        Task<WarehouseStock?> GetByCompositeKeyAsync(Guid warehouseId, Guid productId);
    }
}
