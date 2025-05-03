using IMS.Data.Entities;
using System.Linq.Expressions;

namespace IMS.Data.Repositories.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetAllWithDetailsAsync(Expression<Func<Order, bool>> predicate = null);
        Task<Order?> GetByIdWithDetailsAsync(Guid id);
    }

}
