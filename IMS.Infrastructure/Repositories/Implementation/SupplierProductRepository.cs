using IMS.Infrastructure.Context;
using IMS.Infrastructure.Repositories.Interfaces;
using IMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMS.Infrastructure.Repositories.Implementation
{
    public class SupplierProductRepository : GenericRepository<SupplierProduct>, ISupplierProductRepository
    {
        private readonly InventoryDbContext _context;

        public SupplierProductRepository(InventoryDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task DeleteAsync(Guid supplierId, Guid productId)
        {
            var supplierProduct = await _context.SupplierProducts
                .FirstOrDefaultAsync(sp => sp.SupplierID == supplierId && sp.ProductID == productId);
            if (supplierProduct == null)
            {
                throw new KeyNotFoundException($"SupplierProduct with SupplierID {supplierId} and ProductID {productId} not found.");
            }

            _context.SupplierProducts.Remove(supplierProduct);
        }
    }
}
