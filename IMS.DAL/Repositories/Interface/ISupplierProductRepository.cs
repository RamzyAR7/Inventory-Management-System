using IMS.DAL.Repositories.Interfaces;
using IMS.Domain.Entities;

namespace IMS.DAL.Repositories.Interfaces
{
    public interface ISupplierProductRepository: IGenericRepository<SupplierProduct>
    {
        Task DeleteAsync(Guid supplierId, Guid productId);
    }

}
