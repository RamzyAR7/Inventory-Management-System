using IMS.Data.Context;
using IMS.Data.Entities;
using IMS.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IMS.Data.Repositories.Implementation
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
