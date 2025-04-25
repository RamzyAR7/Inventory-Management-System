using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.DataAccess.Context;
using Inventory_Management_System.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Inventory_Management_System.DataAccess.Repositories
{
    public class ProductRepository:GenericRepository<Product>, IProductRepository
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
                .ToListAsync();
        }
        public async Task<Product> GetAsyncWithNestedIncludesBy(Expression<Func<Product, bool>> predicate)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Suppliers)
                .ThenInclude(sp => sp.Supplier)
                .FirstOrDefaultAsync(predicate);
            if (product == null)
            {
                throw new Exception("Product not found");
            }
            return product;
        }
    }
}
