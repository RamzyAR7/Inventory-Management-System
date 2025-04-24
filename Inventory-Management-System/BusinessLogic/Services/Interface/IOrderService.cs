using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Order;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResDto>> GetAllAsync();
        Task<OrderResDto?> GetByIdAsync(Guid id);
        Task CreateAsync(OrderReqDto orderDto);
        Task UpdateAsync(Guid id, OrderReqDto orderDto);
        Task DeleteAsync(Guid id);
    }
}