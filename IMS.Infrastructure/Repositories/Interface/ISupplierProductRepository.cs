using IMS.Infrastructure.Repositories.Interfaces;
using IMS.Domain.Entities;

namespace IMS.Infrastructure.Repositories.Interfaces
{
    public interface ISupplierProductRepository: IGenericRepository<SupplierProduct>
    {
        Task DeleteAsync(Guid supplierId, Guid productId);
    }

}
