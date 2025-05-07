using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using IMS.DAL.Repositories.Interfaces;
using IMS.DAL.Context;
using IMS.Domain.Entities;

namespace IMS.DAL.Repositories.Implementation
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(InventoryDbContext context) : base(context)
        {

        }

        public async Task<Order?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Warehouse)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderID == id);
        }
    }
}
