using IMS.BAL.DTOs.Warehouse;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface IWarehouseService
    {
        Task<IEnumerable<WarehouseResDto>> GetAllAsync();
        Task<WarehouseResDto?> GetByIdAsync(Guid id);
        Task CreateAsync(WarehouseReqDto warehouseDto);
        Task UpdateAsync(Guid id, WarehouseReqDto warehouseDto);
        Task DeleteAsync(Guid id);
    }
}
