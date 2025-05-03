using IMS.BAL.DTOs.Category;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResDto>> GetAllCategories();
        Task<CategoryResDto> GetCategoryById(Guid id);
        Task CreateCategory(CategoryReqDto categoryDto);
        Task UpdateCategory(Guid id, CategoryReqDto categoryDto);
        Task DeleteCategory(Guid id);
    }
}
