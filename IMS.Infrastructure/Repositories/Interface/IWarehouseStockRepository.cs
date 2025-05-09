using IMS.Domain.Entities;
using System.Linq.Expressions;

namespace IMS.Infrastructure.Repositories.Interfaces
{
    public interface IWarehouseStockRepository : IGenericRepository<WarehouseStock>
    {
        Task DeleteAsync(Guid warehouseId, Guid productId);
    }
}
