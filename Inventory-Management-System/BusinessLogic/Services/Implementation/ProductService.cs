using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _unitOfWork.Products.GetAllAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.Products.GetByIdAsync(e => e.ProductID == id);
        }

        public async Task CreateAsync(Product product)
        {
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.Save();
        }

        public async Task UpdateAsync(Product product)
        {
            _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.Save();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _unitOfWork.Products.DeleteAsync(id);
            await _unitOfWork.Save();
        }
    }
}
