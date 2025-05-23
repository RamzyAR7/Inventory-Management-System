﻿using IMS.Application.DTOs.Category;

namespace IMS.Application.Services.Interface
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
