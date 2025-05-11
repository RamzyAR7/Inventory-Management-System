using AutoMapper;
using IMS.Application.DTOs.Category;
using IMS.Application.Services.Interface;
using IMS.Infrastructure.UnitOfWork;
using IMS.Domain.Entities;
using System.CodeDom;
using IMS.Application.SharedServices.Interface;

namespace IMS.Application.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDashboardUpdateNotifier _dashboardUpdateNotifier;

        public CategoryService(IMapper mapper, IUnitOfWork unitOfWork, IDashboardUpdateNotifier dashboardUpdateNotifier)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _dashboardUpdateNotifier = dashboardUpdateNotifier;
        }

        public async Task<IEnumerable<CategoryResDto>> GetAllCategories()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync(c=> c.Products);
            return _mapper.Map<IEnumerable<CategoryResDto>>(categories);
        }
        public async Task<CategoryResDto> GetCategoryById(Guid id)
        {
            var category = await _unitOfWork.Categories.GetByExpressionAsync(c => c.CategoryID == id, c => c.Products);
            if (category == null)
            {
                return null;
            }
            return _mapper.Map<CategoryResDto>(category);
        }
        public async Task CreateCategory(CategoryReqDto categoryDto)
        {
            var existingCategory = await _unitOfWork.Categories.GetByExpressionAsync(c => c.CategoryName == categoryDto.CategoryName);
            if (existingCategory != null)
                throw new InvalidOperationException("A category with this name already exists");
            var category = _mapper.Map<Category>(categoryDto);
            category.CategoryID = Guid.NewGuid();
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveAsync();
            await _dashboardUpdateNotifier.NotifyDashboardUpdateAsync();
        }
        public async Task UpdateCategory(Guid id, CategoryReqDto categoryDto)
        {
            var category = await _unitOfWork.Categories.GetByExpressionAsync(c => c.CategoryID == id);
            if (category == null)
                throw new NotFoundException($"Category with ID {id} not found");
            _mapper.Map(categoryDto, category);
            await _unitOfWork.Categories.UpdateAsync(category);
            await _unitOfWork.SaveAsync();
            await _dashboardUpdateNotifier.NotifyDashboardUpdateAsync();
        }
        public async Task DeleteCategory(Guid id)
        {
            var category = await _unitOfWork.Categories.GetByExpressionAsync(c => c.CategoryID == id);
            if (category == null)
                throw new NotFoundException($"Category with ID {id} not found");
            await _unitOfWork.Categories.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            await _dashboardUpdateNotifier.NotifyDashboardUpdateAsync();
        }
    }
}

