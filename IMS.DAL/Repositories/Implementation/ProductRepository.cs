using IMS.DAL.Context;
using IMS.DAL.Entities;
using IMS.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IMS.DAL.Repositories.Implementation
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

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsyncWithNestedIncludes(
            int pageNumber,
            int pageSize,
            Expression<Func<Product, bool>> predicate = null,
            Expression<Func<Product, object>> orderBy = null,
            bool sortDescending = false)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));
            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Suppliers)
                    .ThenInclude(sp => sp.Supplier)
                .Include(p => p.WarehouseStocks)
                    .ThenInclude(ws => ws.Warehouse)
                .AsNoTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var totalCount = await query.CountAsync();

            if (orderBy != null)
            {
                query = sortDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items ?? Enumerable.Empty<Product>(), totalCount);
        }
    }
}
