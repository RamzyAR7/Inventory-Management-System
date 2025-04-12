using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderDetailService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<OrderDetail>> GetAllAsync()
        {
            return await _unitOfWork.OrderDetails.GetAllAsync();
        }

        public async Task<OrderDetail?> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.OrderDetails.GetByIdAsync(id);
        }

        public async Task CreateAsync(OrderDetail orderDetail)
        {
            await _unitOfWork.OrderDetails.AddAsync(orderDetail);
            await _unitOfWork.Save();
        }

        public async Task UpdateAsync(OrderDetail orderDetail)
        {
            _unitOfWork.OrderDetails.UpdateAsync(orderDetail);
            await _unitOfWork.Save();
        }

        public async Task DeleteAsync(Guid id)
        {
            await _unitOfWork.OrderDetails.DeleteAsync(id);
            await _unitOfWork.Save();
        }
    }
}
