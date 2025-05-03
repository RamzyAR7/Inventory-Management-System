using IMS.Data.Entities;
using IMS.Data.Repositories.Interfaces;

namespace IMS.Data.Repositories.Interfaces
{
    public interface ISupplierProductRepository: IGenericRepository<SupplierProduct>
    {
        Task DeleteAsync(Guid supplierId, Guid productId);
    }

}
