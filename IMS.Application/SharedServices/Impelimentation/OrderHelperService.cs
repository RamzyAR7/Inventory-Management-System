using IMS.Application.DTOs.Order.Request;
using IMS.Application.Models;
using IMS.Application.SharedServices.Interface;
using IMS.Infrastructure.UnitOfWork;
using IMS.Domain.Entities;
using IMS.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Application.SharedServices.Impelimentation
{
    public class OrderHelperService:IOrderHelperService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWhoIsUserLoginService _userLoginService;
        private readonly ILogger<OrderHelperService> _logger;
        public OrderHelperService(IUnitOfWork unitOfWork, IWhoIsUserLoginService userLoginService, ILogger<OrderHelperService> logger)
        {
            _unitOfWork = unitOfWork;
            _userLoginService = userLoginService;
            _logger = logger;
        }

        public async Task<List<ProductsWithCategoryViewModel>> GetProductsByWarehouseAndCategoryAsync(Guid warehouseId, Guid? categoryId)
        {
            try
            {
                // Validate warehouse access
                var userRole = await _userLoginService.GetCurrentUserRole();
                var userId = await _userLoginService.GetCurrentUserId();
                var accessibleWarehouseIds = await _userLoginService.GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

                if (!accessibleWarehouseIds.Contains(warehouseId))
                    throw new UnauthorizedAccessException("You do not have access to this warehouse.");

                // Fetch products available in the warehouse
                var stocks = await _unitOfWork.WarehouseStocks
                    .FindAsync(ws => ws.WarehouseID == warehouseId && ws.StockQuantity > 0);

                var productIds = stocks.Select(ws => ws.ProductID).ToList();

                // Fetch products that are active and in stock
                Expression<Func<Product, bool>> predicate = p =>
                    productIds.Contains(p.ProductID) &&
                    p.IsActive;

                if (categoryId.HasValue)
                    predicate = p => productIds.Contains(p.ProductID) && p.IsActive && p.CategoryID == categoryId;

                var products = await _unitOfWork.Products.FindAsync(predicate, p => p.Category);

                var result = products.Select(p => new ProductsWithCategoryViewModel
                {
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    CategoryName = p.Category?.CategoryName ?? "N/A",
                    Price = p.Price,
                    StockQuantity = _unitOfWork.WarehouseStocks
                        .FirstOrDefaultAsync(ws => ws.WarehouseID == warehouseId && ws.ProductID == p.ProductID)
                        .Result?.StockQuantity ?? 0
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products for WarehouseID {WarehouseID}, CategoryID {CategoryID}.", warehouseId, categoryId);
                throw;
            }
        }
        public async Task<(bool isValid, string errorMessage, OrderDetail orderDetail)> ValidateAndAddProductAsync(Guid warehouseId, Guid productId, int quantity, Guid userId)
        {
            try
            {
                // Validate warehouse access
                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserID == userId);
                if (user == null)
                    return (false, "User not found.", null);

                var accessibleWarehouseIds = await _userLoginService.GetAccessibleWarehouseIdsAsync(user.Role, userId);
                if (!accessibleWarehouseIds.Contains(warehouseId))
                    return (false, "You do not have access to this warehouse.", null);

                // Validate product
                var product = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.ProductID == productId);
                if (product == null || !product.IsActive)
                    return (false, "Product is invalid or inactive.", null);

                // Validate stock
                var stock = await _unitOfWork.WarehouseStocks
                    .FirstOrDefaultAsync(ws => ws.WarehouseID == warehouseId && ws.ProductID == productId);
                if (stock == null || stock.StockQuantity < quantity)
                    return (false, $"Insufficient stock for product {product.ProductName}. Available: {stock?.StockQuantity ?? 0}, Requested: {quantity}", null);

                if (quantity <= 0)
                    return (false, "Quantity must be greater than 0.", null);

                // Create OrderDetail
                var orderDetail = new OrderDetail
                {
                    ProductID = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * quantity
                };

                return (true, string.Empty, orderDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating product addition: ProductID {ProductID}, WarehouseID {WarehouseID}.", productId, warehouseId);
                return (false, "An error occurred: " + ex.Message, null);
            }
        }
        public async Task ValidateOrderDtoAsync(OrderReqDto orderDto)
        {
            if (orderDto.OrderDetails == null || !orderDto.OrderDetails.Any())
                throw new InvalidOperationException("At least one order detail is required.");

            var warehouse = await _unitOfWork.Warehouses.GetByExpressionAsync(w => w.WarehouseID == orderDto.WarehouseID);
            if (warehouse == null)
                throw new InvalidOperationException("Warehouse not found.");

            var customer = await _unitOfWork.Customers.GetByExpressionAsync(c => c.CustomerID == orderDto.CustomerID);
            if (customer == null)
                throw new InvalidOperationException("Customer not found.");

            var productIds = orderDto.OrderDetails.Select(d => d.ProductID).Distinct().ToList();
            var products = await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.ProductID));
            if (products.Count() != productIds.Count)
                throw new InvalidOperationException("One or more products not found.");
        }

        public async Task ValidateUserAccessAsync(Guid warehouseId)
        {
            var userRole = await _userLoginService.GetCurrentUserRole();
            var userId = await _userLoginService.GetCurrentUserId();
            var managerWarehouseIds = await _userLoginService.GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

            if (userRole != "Admin" && !managerWarehouseIds.Contains(warehouseId))
                throw new UnauthorizedAccessException("You can only access your assigned warehouses.");
        }

        public bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                (OrderStatus.Pending, OrderStatus.Confirmed) => true,
                (OrderStatus.Pending, OrderStatus.Cancelled) => true,
                (OrderStatus.Confirmed, OrderStatus.Pending) => true,
                (OrderStatus.Confirmed, OrderStatus.Shipped) => true,
                (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
                (OrderStatus.Shipped, OrderStatus.Delivered) => true,
                (OrderStatus.Shipped, OrderStatus.Pending) => true,
                (OrderStatus.Shipped, OrderStatus.Confirmed) => true,
                (OrderStatus.Cancelled, OrderStatus.Pending) => true,
                _ => false
            };
        }
    }
}
