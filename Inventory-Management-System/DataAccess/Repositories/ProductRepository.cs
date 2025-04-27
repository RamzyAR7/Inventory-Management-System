using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.DataAccess.Context;
using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Inventory_Management_System.DataAccess.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly InventoryDbContext _context;

        public ProductRepository(InventoryDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsyncWithNestedIncludes()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Suppliers)
                .ThenInclude(sp => sp.Supplier)
                .Include(p => p.WarehouseStocks)
                .ThenInclude(ws => ws.Warehouse)
                .ToListAsync();
        }

        public async Task<Product> GetAsyncWithNestedIncludesBy(Expression<Func<Product, bool>> predicate)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Suppliers)
                .ThenInclude(sp => sp.Supplier)
                .Include(p => p.WarehouseStocks)
                .ThenInclude(ws => ws.Warehouse)
                .FirstOrDefaultAsync(predicate);
            if (product == null)
            {
                throw new Exception("Product not found");
            }
            return product;
        }

        public async Task<Product?> FindAsyncWithNestedIncludes(Expression<Func<Product, bool>> predicate)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Suppliers)
                .ThenInclude(sp => sp.Supplier)
                .Include(p => p.WarehouseStocks)
                .ThenInclude(ws => ws.Warehouse)
                .FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<Product>> GetAllForWarehousesAsync(List<Guid> warehouseIds)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Suppliers)
                .ThenInclude(sp => sp.Supplier)
                .Include(p => p.WarehouseStocks)
                .ThenInclude(ws => ws.Warehouse)
                .Where(p => p.WarehouseStocks.Any(ws => warehouseIds.Contains(ws.WarehouseID)))
                .ToListAsync();
        }
    }
}
