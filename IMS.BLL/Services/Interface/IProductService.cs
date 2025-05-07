using IMS.BLL.DTOs.Products;
using IMS.DAL.Entities;

namespace IMS.BLL.Services.Interface
{
    public interface IProductService
    {
        Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedProductsAsync(
                int pageNumber = 1,
                int pageSize = 10,
                Guid? categoryId = null,
                string sortBy = "ProductName",
                bool sortDescending = false,
                string userId = null,
                string userRole = null);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(Guid id);
        Task CreateAsync(ProductReqDto productDto);
        Task UpdateAsync(Guid id, ProductReqDto productDto);
        Task DeleteAsync(Guid id);
    }
}
