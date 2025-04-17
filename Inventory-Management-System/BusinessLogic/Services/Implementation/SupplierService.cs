using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SupplierService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return await _unitOfWork.Suppliers.GetAllAsync();
        }

        public async Task<Supplier?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.Suppliers.GetByIdAsync(id);
        }

        public async Task CreateAsync(Supplier supplier)
        {
            await _unitOfWork.Suppliers.AddAsync(supplier);
            await _unitOfWork.Save();
        }

        public async Task UpdateAsync(Supplier supplier)
        {
            _unitOfWork.Suppliers.UpdateAsync(supplier);
            await _unitOfWork.Save();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _unitOfWork.Suppliers.DeleteAsync(id);
            await _unitOfWork.Save();
        }
    }

}
