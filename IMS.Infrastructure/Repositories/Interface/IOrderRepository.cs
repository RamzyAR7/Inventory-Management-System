using IMS.Domain.Entities;
using System.Linq.Expressions;

namespace IMS.Infrastructure.Repositories.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Order?> GetByIdWithDetailsAsync(Guid id);
    }

}
