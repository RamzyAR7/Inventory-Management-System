using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class CustomerOrderService : ICustomerOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerOrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<CustomerOrder>> GetAllAsync()
        {
            return await _unitOfWork.CustomerOrders.GetAllAsync();
        }

        public async Task<CustomerOrder?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.CustomerOrders.GetByIdAsync(e => e.CustomerOrderID == id);
        }

        public async Task CreateAsync(CustomerOrder customerOrder)
        {
            await _unitOfWork.CustomerOrders.AddAsync(customerOrder);
            await _unitOfWork.Save();
        }

        public async Task UpdateAsync(CustomerOrder customerOrder)
        {
            _unitOfWork.CustomerOrders.UpdateAsync(customerOrder);
            await _unitOfWork.Save();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _unitOfWork.CustomerOrders.DeleteAsync(id);
            await _unitOfWork.Save();
        }
    }

}
