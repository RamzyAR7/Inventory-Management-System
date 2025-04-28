using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Products;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(Guid id);
        Task CreateAsync(ProductReqDto productDto);
        Task UpdateAsync(Guid id, ProductReqDto productDto);
        Task<List<ProductReqDto>> GetProductsByWarehouseAsync(Guid warehouseId);
        Task DeleteAsync(Guid id);
        Task AssignSupplierFromAnotherProductAsync(Guid sourceProductId, Guid targetProductId);
    }
}
