using IMS.DAL.Context;
using IMS.DAL.Entities;
using IMS.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IMS.DAL.Repositories.Implementation
{
    public class SuppliersRepository : GenericRepository<Supplier>, ISuppliersRepository
    {
        private readonly InventoryDbContext _context;
        public SuppliersRepository(InventoryDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Supplier>> GetAllSuppliersWithProducts()
        {
            return await _context.Suppliers
                        .Include(s => s.SupplierProducts)
                        .ThenInclude(sp => sp.Product)
                        .ToListAsync();
        }

        public async Task<Supplier> GetSupplierBy(Expression<Func<Supplier, bool>> predicate)
        {
            var supplier = await _context.Suppliers
                            .Include(s => s.SupplierProducts)
                            .ThenInclude(sp => sp.Product)
                            .FirstOrDefaultAsync(predicate);
            if (supplier == null)
            {
                throw new Exception("Supplier not found");
            }
            return supplier;
        }
    }
}
