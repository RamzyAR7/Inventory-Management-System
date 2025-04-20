using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _unitOfWork.Orders.GetAllAsync();
        }

        public async Task<Order?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.Orders.GetByIdAsync(e => e.OrderID ==  id);
        }

        public async Task CreateAsync(Order order)
        {
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.Save();
        }

        public async Task UpdateAsync(Order order)
        {
            _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.Save();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _unitOfWork.Orders.DeleteAsync(id);
            await _unitOfWork.Save();
        }
    }
}
