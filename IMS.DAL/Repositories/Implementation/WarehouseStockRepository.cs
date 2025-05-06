using IMS.DAL.Context;
using IMS.DAL.Entities;
using IMS.DAL.Repositories.Interfaces;
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

        public async Task<WarehouseStock?> GetByCompositeKeyAsync(Guid warehouseId, Guid productId)
        {
            return await _context.WarehouseStocks
                .FirstOrDefaultAsync(ws => ws.WarehouseID == warehouseId && ws.ProductID == productId);
        }

        public async Task AddAsync(WarehouseStock entity)
        {
            await _context.WarehouseStocks.AddAsync(entity);
        }

        public async Task UpdateAsync(WarehouseStock entity)
        {
            _context.WarehouseStocks.Update(entity);
        }

        public IQueryable<WarehouseStock> Find(Expression<Func<WarehouseStock, bool>> predicate)
        {
            return _context.WarehouseStocks.Where(predicate);
        }
    }
}
