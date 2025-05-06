using IMS.DAL.Entities;
using System.Linq.Expressions;

namespace IMS.DAL.Repositories.Interfaces
{
    public interface IWarehouseStockRepository : IGenericRepository<WarehouseStock>
    {
        Task DeleteAsync(Guid warehouseId, Guid productId);
        Task<WarehouseStock?> GetByCompositeKeyAsync(Guid warehouseId, Guid productId);
        Task UpdateAsync(WarehouseStock entity);
        Task AddAsync(WarehouseStock entity);
        IQueryable<WarehouseStock> Find(Expression<Func<WarehouseStock, bool>> predicate);
    }
}
