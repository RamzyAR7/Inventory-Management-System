using AutoMapper;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Order;
using Inventory_Management_System.Models.DTOs.Order.Responce;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseDto>> GetAllAsync();
        Task<OrderDetailResponseDto?> GetByIdAsync(Guid id);
        Task CreateAsync(OrderReqDto orderDto);
        Task UpdateStatusAsync(Guid orderId, OrderStatus newStatus);
        Task EditAsync(Guid orderId, OrderReqDto orderDto);
        Task<List<Guid>> GetAccessibleWarehouseIdsAsync(string role, Guid userId);
        Task<List<Product>> GetProductsByWarehouseAndCategoryAsync(Guid warehouseId, Guid? categoryId);
        Task<(bool isValid, string errorMessage, OrderDetail orderDetail)> ValidateAndAddProductAsync(Guid warehouseId, Guid productId, int quantity, Guid userId);
    }
}
