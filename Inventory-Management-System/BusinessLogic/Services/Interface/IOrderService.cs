using AutoMapper;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Order.Request;
using Inventory_Management_System.Models.DTOs.Order.Responce;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseDto>> GetAllAsync();
        Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedOrdersAsync(int pageNumber, int pageSize);
        Task<OrderDetailResponseDto?> GetByIdAsync(Guid id);
        Task CreateAsync(OrderReqDto orderDto, Guid userId);
        Task UpdateStatusAsync(Guid orderId, OrderStatus newStatus);
        Task EditAsync(Guid orderId, OrderReqDto orderDto);
        Task<List<Guid>> GetAccessibleWarehouseIdsAsync(string role, Guid userId);
        Task<List<Product>> GetProductsByWarehouseAndCategoryAsync(Guid warehouseId, Guid? categoryId);
        Task<(bool isValid, string errorMessage, OrderDetail orderDetail)> ValidateAndAddProductAsync(Guid warehouseId, Guid productId, int quantity, Guid userId);
    }
}
