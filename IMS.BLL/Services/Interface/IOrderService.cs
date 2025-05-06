using AutoMapper;
using IMS.BLL.DTOs.Order.Request;
using IMS.BLL.DTOs.Order.Responce;
using IMS.DAL.Entities;
using IMS.BLL.Services.Interface;

namespace IMS.BLL.Services.Interface
{
    public interface IOrderService
    {
        Task<(IEnumerable<OrderResponseDto> Items, int TotalCount)> GetPagedOrdersAsync(int pageNumber, int pageSize, OrderStatus? statusFilter = null);
        //Task<IEnumerable<OrderResponseDto>> GetAllAsync();
        Task<OrderDetailResponseDto?> GetByIdAsync(Guid id);
        Task CreateAsync(OrderReqDto orderDto, Guid userId);
        Task UpdateStatusAsync(Guid orderId, OrderStatus newStatus);
        Task EditAsync(Guid orderId, OrderReqDto orderDto);
        Task DeleteAsync(Guid id);
        Task<List<Guid>> GetAccessibleWarehouseIdsAsync(string role, Guid userId);
        Task<List<Product>> GetProductsByWarehouseAndCategoryAsync(Guid warehouseId, Guid? categoryId);
        Task<(bool isValid, string errorMessage, OrderDetail orderDetail)> ValidateAndAddProductAsync(Guid warehouseId, Guid productId, int quantity, Guid userId);
    }
}
