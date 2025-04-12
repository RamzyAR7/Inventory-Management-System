using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WarehouseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Warehouse>> GetAllAsync()
        {
            return await _unitOfWork.Warehouses.GetAllAsync();
        }

        public async Task<Warehouse?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.Warehouses.GetByIdAsync(id);
        }

        public async Task CreateAsync(Warehouse warehouse)
        {
            await _unitOfWork.Warehouses.AddAsync(warehouse);
            await _unitOfWork.Save();
        }

        public async Task UpdateAsync(Warehouse warehouse)
        {
            _unitOfWork.Warehouses.UpdateAsync(warehouse);
            await _unitOfWork.Save();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _unitOfWork.Warehouses.DeleteAsync(id);
            await _unitOfWork.Save();
        }
    }
}
