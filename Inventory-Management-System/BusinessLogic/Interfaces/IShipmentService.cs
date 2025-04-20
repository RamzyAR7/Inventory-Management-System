using Inventory_Management_System.Entities;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface IShipmentService
    {
        Task<IEnumerable<Shipment>> GetAllAsync();
        Task<Shipment?> GetByIdAsync(Guid id);
        Task CreateAsync(Shipment shipment);
        Task UpdateAsync(Shipment shipment);
        Task DeleteAsync(Guid id);
    }
}
