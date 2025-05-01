using Inventory_Management_System.Entities;
using System.Linq.Expressions;

namespace Inventory_Management_System.BusinessLogic.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetAllWithDetailsAsync(Expression<Func<Order, bool>> predicate = null);
        Task<Order?> GetByIdWithDetailsAsync(Guid id);
    }

}
