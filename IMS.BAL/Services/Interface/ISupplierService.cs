using IMS.BAL.DTOs.Supplier;
using IMS.Data.Entities;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface ISupplierService
    {
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<Supplier> GetByIdAsync(Guid id);
        Task CreateAsync(SupplierReqDto supplierDto);
        Task UpdateAsync(Guid id, SupplierReqDto supplierDto);
        Task DeleteAsync(Guid id);
    }
}
