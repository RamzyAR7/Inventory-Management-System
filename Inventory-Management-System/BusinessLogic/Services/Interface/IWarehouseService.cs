using Inventory_Management_System.Entities;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface IWarehouseService
    {
        Task<IEnumerable<Warehouse>> GetAllAsync();
        Task<Warehouse?> GetByIdAsync(Guid id);
        Task CreateAsync(Warehouse warehouse);
        Task UpdateAsync(Warehouse warehouse);
        Task DeleteAsync(Guid id);
    }
}
