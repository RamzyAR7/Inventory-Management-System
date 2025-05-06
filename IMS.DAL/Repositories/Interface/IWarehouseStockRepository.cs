using IMS.DAL.Entities;
using System.Linq.Expressions;

namespace IMS.DAL.Repositories.Interfaces
{
    public interface IWarehouseStockRepository : IGenericRepository<WarehouseStock>
    {
        Task DeleteAsync(Guid warehouseId, Guid productId);
    }
}
