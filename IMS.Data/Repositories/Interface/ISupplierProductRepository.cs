using IMS.DAL.Entities;
using IMS.DAL.Repositories.Interfaces;

namespace IMS.DAL.Repositories.Interfaces
{
    public interface ISupplierProductRepository: IGenericRepository<SupplierProduct>
    {
        Task DeleteAsync(Guid supplierId, Guid productId);
    }

}
