using AutoMapper;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Category;
using System.CodeDom;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CategoryResDto>> GetAllCategories()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryResDto>>(categories);
        }
        public async Task<CategoryResDto> GetCategoryById(Guid id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(c => c.CategoryID == id, c => c.Products);
            if (category == null)
            {
                return null;
            }
            return _mapper.Map<CategoryResDto>(category);
        }
        public async Task CreateCategory(CategoryReqDto categoryDto)
        {
            var existingCategory = await _unitOfWork.Categories.GetByIdAsync(c => c.CategoryName == categoryDto.CategoryName);
            if (existingCategory != null)
                throw new InvalidOperationException("A category with this name already exists");
            var category = _mapper.Map<Category>(categoryDto);
            category.CategoryID = Guid.NewGuid();
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.Save();
        }
        public async Task UpdateCategory(Guid id, CategoryReqDto categoryDto)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(c => c.CategoryID == id);
            if (category == null)
                throw new NotFoundException($"Category with ID {id} not found");
            _mapper.Map(categoryDto, category);
            await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.Save();
        }
        public async Task DeleteCategory(Guid id)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(c => c.CategoryID == id);
            if (category == null)
                throw new NotFoundException($"Category with ID {id} not found");
            await _unitOfWork.Categories.DeleteAsync(id);
            await _unitOfWork.Save();
        }
    }
}

