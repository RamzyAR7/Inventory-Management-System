using Inventory_Management_System.DataAccess.Context;
using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Inventory_Management_System.BusinessLogic.Interfaces;

namespace Inventory_Management_System.DataAccess.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(InventoryDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetAllWithDetailsAsync(Expression<Func<Order, bool>> predicate = null)
        {
            IQueryable<Order> query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Warehouse)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product);

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return await query.ToListAsync();
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
