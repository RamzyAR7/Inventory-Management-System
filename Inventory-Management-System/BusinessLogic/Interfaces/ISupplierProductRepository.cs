using Inventory_Management_System.Entities;

namespace Inventory_Management_System.BusinessLogic.Interfaces
{
    public interface ISupplierProductRepository: IGenericRepository<SupplierProduct>
    {
        Task DeleteAsync(Guid supplierId, Guid productId);
    }

}
