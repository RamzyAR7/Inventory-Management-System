using AutoMapper;
using IMS.BLL.DTOs.Order.Request;
using IMS.BLL.DTOs.Order.Responce;
using IMS.Domain.Enums;
using IMS.BLL.Services.Interface;

namespace IMS.BLL.Services.Interface
{
    public interface IOrderService
    {
        Task<(IEnumerable<OrderResponseDto> Items, int TotalCount)> GetPagedOrdersAsync(
                    int pageNumber,
                    int pageSize,
                    OrderStatus? statusFilter = null,
                    string sortBy = "OrderDate",
                    bool sortDescending = false);
        Task<OrderDetailResponseDto?> GetByIdAsync(Guid id);
        Task CreateAsync(OrderReqDto orderDto);
        Task UpdateStatusAsync(Guid orderId, OrderStatus newStatus);
        Task EditAsync(Guid orderId, OrderReqDto orderDto);
        Task DeleteAsync(Guid id);
    }
}
