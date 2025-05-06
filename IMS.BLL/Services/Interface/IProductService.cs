using IMS.BLL.DTOs.Products;
using IMS.DAL.Entities;

namespace IMS.BLL.Services.Interface
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
