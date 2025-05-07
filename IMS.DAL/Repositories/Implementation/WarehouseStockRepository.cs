using IMS.DAL.Context;
using IMS.DAL.Repositories.Interfaces;
using IMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IMS.DAL.Repositories.Implementation
{
    public class WarehouseStockRepository : GenericRepository<WarehouseStock>, IWarehouseStockRepository
    {
        private readonly InventoryDbContext _context;

        public WarehouseStockRepository(InventoryDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task DeleteAsync(Guid warehouseId, Guid productId)
        {
            var warehouseStock = await _context.WarehouseStocks
                .FirstOrDefaultAsync(ws => ws.WarehouseID == warehouseId && ws.ProductID == productId);
            if (warehouseStock == null)
            {
                throw new KeyNotFoundException($"WarehouseStock with WarehouseID {warehouseId} and ProductID {productId} not found.");
            }

            _context.WarehouseStocks.Remove(warehouseStock);
        }
    }
}
