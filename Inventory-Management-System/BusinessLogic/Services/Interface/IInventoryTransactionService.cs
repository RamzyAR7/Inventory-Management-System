using Inventory_Management_System.Entities;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface IInventoryTransactionService
    {
        Task<IEnumerable<InventoryTransaction>> GetAllAsync();
        Task<InventoryTransaction?> GetByIdAsync(Guid id);
        Task CreateAsync(InventoryTransaction transaction);
        Task UpdateAsync(InventoryTransaction transaction);
        Task DeleteAsync(Guid id);
    }
}
