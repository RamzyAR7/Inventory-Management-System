using IMS.Application.DTOs.Order.Request;
using IMS.Application.Models;
using IMS.Domain.Entities;
using IMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.SharedServices.Interface
{
    public interface IOrderHelperService
    {
        Task<List<ProductsWithCategoryViewModel>> GetProductsByWarehouseAndCategoryAsync(Guid warehouseId, Guid? categoryId);
        Task<(bool isValid, string errorMessage, OrderDetail orderDetail)> ValidateAndAddProductAsync(Guid warehouseId, Guid productId, int quantity, Guid userId);
        bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus);
        Task ValidateUserAccessAsync(Guid warehouseId);
        Task ValidateOrderDtoAsync(OrderReqDto orderDto);
    }
}
