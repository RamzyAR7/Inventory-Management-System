using AutoMapper;
using IMS.Application.DTOs.Order.Request;
using IMS.Application.DTOs.Order.Responce;
using IMS.Application.Services.Interface;
using IMS.Application.SharedServices.Interface;
using IMS.Domain.Enums;
using IMS.Infrastructure.UnitOfWork;
using IMS.Domain;
using IMS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IMS.Application.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWhoIsUserLoginService _userLoginService;
        private readonly IOrderHelperService _orderHelperService;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IWhoIsUserLoginService userLoginService,
            IOrderHelperService orderHelperService,
            ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _userLoginService = userLoginService;
            _orderHelperService = orderHelperService;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<(IEnumerable<OrderResponseDto> Items, int TotalCount)> GetPagedOrdersAsync(
            int pageNumber,
            int pageSize,
            OrderStatus? statusFilter = null,
            string sortBy = "OrderDate",
            bool sortDescending = false)
        {
            try
            {
                var userRole = await _userLoginService.GetCurrentUserRole();
                var userId = await _userLoginService.GetCurrentUserId();
                var managerWarehouseIds = await _userLoginService.GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

                var includes = new Expression<Func<Order, object>>[]
                {
                    o => o.Customer,
                    o => o.Warehouse,
                    o => o.CreatedByUser
                };

                Expression<Func<Order, bool>> predicate = null;

                if (userRole != "Admin")
                {
                    predicate = o => managerWarehouseIds.Contains(o.WarehouseID);
                }

                if (statusFilter.HasValue)
                {
                    var statusPredicate = (Expression<Func<Order, bool>>)(o => o.Status == statusFilter.Value);
                    predicate = predicate == null ? statusPredicate : CombinePredicates(predicate, statusPredicate);
                }

                Expression<Func<Order, object>> orderBy;
                switch (sortBy.ToLower())
                {
                    case "customername":
                        orderBy = o => o.Customer.FullName;
                        break;
                    case "warehousename":
                        orderBy = o => o.Warehouse.WarehouseName;
                        break;
                    case "totalamount":
                        orderBy = o => o.TotalAmount;
                        break;
                    case "status":
                        orderBy = o => o.Status;
                        break;
                    case "createdbyusername":
                        orderBy = o => o.CreatedByUser.UserName;
                        break;
                    default:
                        orderBy = o => o.OrderDate;
                        break;
                }
                var (orders, totalCount) = await _unitOfWork.Orders.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    predicate,
                    orderBy,
                    sortDescending,
                    includes);

                var orderDtos = _mapper.Map<IEnumerable<OrderResponseDto>>(orders);
                _logger.LogInformation("GetPagedOrdersAsync - Retrieved {ItemCount} orders, TotalCount: {TotalCount}", orders.Count(), totalCount);
                return (orderDtos, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPagedOrdersAsync - Error retrieving orders: {Message}", ex.Message);
                throw;
            }
        }

        private Expression<Func<T, bool>> CombinePredicates<T>(
            Expression<Func<T, bool>> predicate1,
            Expression<Func<T, bool>> predicate2)
        {
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.AndAlso(
                Expression.Invoke(predicate1, parameter),
                Expression.Invoke(predicate2, parameter));
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public async Task<OrderDetailResponseDto?> GetByIdAsync(Guid id)
        {
            var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Order not found: {OrderID}.", id);
                return null;
            }

            var userRole = await _userLoginService.GetCurrentUserRole();
            var userId = await _userLoginService.GetCurrentUserId();
            var managerWarehouseIds = await _userLoginService.GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

            if (userRole != "Admin" && !managerWarehouseIds.Contains(order.WarehouseID))
            {
                _logger.LogWarning("Unauthorized access to order {OrderID} by user {UserID}.", id, userId);
                throw new UnauthorizedAccessException("You can only view orders for accessible warehouses.");
            }

            return _mapper.Map<OrderDetailResponseDto>(order);
        }

        public async Task CreateAsync(OrderReqDto orderDto)
        {
            try
            {
                // Validate OrderDetails
                if (orderDto.OrderDetails == null || !orderDto.OrderDetails.Any())
                    throw new InvalidOperationException("At least one product is required in the order.");

                // Validate for duplicate ProductIDs
                var productIds = orderDto.OrderDetails.Select(d => d.ProductID).ToList();
                if (productIds.Distinct().Count() != productIds.Count)
                    throw new InvalidOperationException("Duplicate products are not allowed in the order.");

                // Mapping OrderReqDto to Order
                var userId = await _userLoginService.GetCurrentUserId();
                var order = _mapper.Map<Order>(orderDto);
                order.CreatedByUserID = Guid.Parse(userId);

                // Clear OrderDetails to avoid duplicates from AutoMapper
                order.OrderDetails = new List<OrderDetail>();

                // Validate warehouse access
                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserID == order.CreatedByUserID);
                if (user == null)
                    throw new InvalidOperationException("User not found.");

                var accessibleWarehouseIds = await _userLoginService.GetAccessibleWarehouseIdsAsync(user.Role, order.CreatedByUserID);
                if (!accessibleWarehouseIds.Contains(order.WarehouseID))
                    throw new UnauthorizedAccessException("You do not have access to this warehouse.");

                // Validate customer and warehouse
                var customer = await _unitOfWork.Customers.FirstOrDefaultAsync(c => c.CustomerID == order.CustomerID);
                if (customer == null || !customer.IsActive)
                    throw new InvalidOperationException("Invalid or inactive customer.");

                var warehouse = await _unitOfWork.Warehouses.FirstOrDefaultAsync(w => w.WarehouseID == order.WarehouseID);
                if (warehouse == null)
                    throw new InvalidOperationException("Warehouse not found.");

                // Validate and add order details
                decimal totalAmount = 0;
                foreach (var detailDto in orderDto.OrderDetails)
                {
                    var (isValid, errorMessage, orderDetail) = await _orderHelperService.ValidateAndAddProductAsync(
                        order.WarehouseID, detailDto.ProductID, detailDto.Quantity, order.CreatedByUserID);
                    if (!isValid)
                        throw new InvalidOperationException(errorMessage);

                    orderDetail.OrderID = order.OrderID;
                    orderDetail.OrderDetailID = Guid.NewGuid(); // Ensure unique ID for each OrderDetail
                    order.OrderDetails.Add(orderDetail);
                    totalAmount += orderDetail.TotalPrice;
                }

                order.TotalAmount = totalAmount;

                // Clear any tracked OrderDetail entities to avoid conflicts
                var trackedOrderDetails = _unitOfWork.Context.ChangeTracker.Entries<OrderDetail>().ToList();
                foreach (var entry in trackedOrderDetails)
                {
                    entry.State = EntityState.Detached;
                }

                // Add order to context
                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order.");
                throw;
            }
        }

        public async Task UpdateStatusAsync(Guid orderId, OrderStatus newStatus)
        {
            var order = await _unitOfWork.Orders.GetByExpressionAsync(o => o.OrderID == orderId, o => o.OrderDetails, o => o.Customer);
            if (order == null)
                throw new InvalidOperationException("Order not found.");

            await _orderHelperService.ValidateUserAccessAsync(order.WarehouseID);
            var isValidStatus = _orderHelperService.IsValidStatusTransition(order.Status, newStatus);

            if (!isValidStatus)
                throw new InvalidOperationException($"Invalid status transition from {order.Status} to {newStatus}.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (newStatus == OrderStatus.Cancelled)
                {
                    var shipment = await _unitOfWork.Shipments.FirstOrDefaultAsync(s => s.OrderID == orderId);
                    if (shipment != null && shipment.Status != ShipmentStatus.Cancelled)
                        throw new InvalidOperationException("Cannot cancel an order with an active shipment.");
                }

                if (order.Status == OrderStatus.Pending && newStatus == OrderStatus.Confirmed)
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        var stock = await _unitOfWork.WarehouseStocks.GetByCompositeKeyAsync(order.WarehouseID, detail.ProductID);
                        if (stock == null || stock.StockQuantity < detail.Quantity)
                            throw new InvalidOperationException($"Insufficient stock for product ID {detail.ProductID}. Available: {stock?.StockQuantity ?? 0}, Requested: {detail.Quantity}");

                        var existingTransaction = await _unitOfWork.InventoryTransactions.FirstOrDefaultAsync(t =>
                            t.OrderID == order.OrderID && t.ProductID == detail.ProductID && t.Type == TransactionType.Out);
                        if (existingTransaction != null)
                            continue;

                        stock.StockQuantity -= detail.Quantity;
                        await _unitOfWork.WarehouseStocks.UpdateAsync(stock);

                        var inventoryTransaction = new InventoryTransaction
                        {
                            TransactionID = Guid.NewGuid(),
                            ProductID = detail.ProductID,
                            WarehouseID = order.WarehouseID,
                            Type = TransactionType.Out,
                            Quantity = detail.Quantity,
                            TransactionDate = DateTime.UtcNow,
                            OrderID = order.OrderID
                        };
                        await _unitOfWork.InventoryTransactions.AddAsync(inventoryTransaction);
                    }
                }
                else if (order.Status == OrderStatus.Confirmed && newStatus == OrderStatus.Shipped)
                {
                    var shipment = await _unitOfWork.Shipments.FirstOrDefaultAsync(s => s.OrderID == orderId);
                    if (shipment == null)
                    {
                        var newShipment = new Shipment
                        {
                            ShipmentID = Guid.NewGuid(),
                            OrderID = orderId,
                            ShippedDate = DateTime.UtcNow,
                            Status = ShipmentStatus.Pending,
                            ItemCount = order.OrderDetails.Count,
                            Destination = order.Customer?.Address ?? "Default Address",
                            DeliveryMethod = DeliveryMethod.Pickup,
                            DeliveryName = "N/A",
                            DeliveryPhoneNumber = "N/A"
                        };
                        await _unitOfWork.Shipments.AddAsync(newShipment);
                    }
                    else if (shipment.Status == ShipmentStatus.Cancelled)
                    {
                        shipment.Status = ShipmentStatus.Pending;
                        shipment.ShippedDate = DateTime.UtcNow;
                        await _unitOfWork.Shipments.UpdateAsync(shipment);
                    }
                }
                else if (newStatus == OrderStatus.Cancelled || newStatus == OrderStatus.Pending && order.Status != OrderStatus.Delivered)
                {
                    if (order.Status == OrderStatus.Confirmed || order.Status == OrderStatus.Shipped)
                    {
                        var shipment = await _unitOfWork.Shipments.GetByExpressionAsync(s => s.OrderID == orderId);
                        if (shipment != null)
                        {
                            await _unitOfWork.Shipments.DeleteAsync(shipment.ShipmentID);
                        }
                        var transactions = await _unitOfWork.InventoryTransactions.FindAsync(t => t.OrderID == orderId && t.Type == TransactionType.Out);
                        foreach (var trans in transactions)
                        {
                            var stock = await _unitOfWork.WarehouseStocks.GetByCompositeKeyAsync(order.WarehouseID, trans.ProductID);
                            if (stock != null)
                            {
                                stock.StockQuantity += trans.Quantity;
                                await _unitOfWork.WarehouseStocks.UpdateAsync(stock);
                            }
                            await _unitOfWork.InventoryTransactions.DeleteAsync(trans.TransactionID);
                        }
                    }

                }
                else if (order.Status == OrderStatus.Shipped && newStatus == OrderStatus.Confirmed)
                {
                    var shipment = await _unitOfWork.Shipments.GetByExpressionAsync(s => s.OrderID == orderId);
                    if (shipment != null)
                    {
                        await _unitOfWork.Shipments.DeleteAsync(shipment.ShipmentID);
                    }
                }
                order.Status = newStatus;
                await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Order status updated: {OrderID}, New Status: {Status}.", orderId, newStatus);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating order status: {OrderID}, Message: {Message}.", orderId, ex.Message);
                throw;
            }
        }

        public async Task EditAsync(Guid orderId, OrderReqDto orderDto)
        {
            if (orderId != orderDto.OrderID)
                throw new InvalidOperationException("Order ID mismatch.");

            await _orderHelperService.ValidateUserAccessAsync(orderDto.WarehouseID);
            await _orderHelperService.ValidateOrderDtoAsync(orderDto);

            var order = await _unitOfWork.Orders.GetByExpressionAsync(o => o.OrderID == orderId, o => o.OrderDetails);
            if (order == null)
                throw new InvalidOperationException("Order not found.");

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException("Can only edit orders in Pending status.");

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                
                order.CustomerID = orderDto.CustomerID;
                order.WarehouseID = orderDto.WarehouseID;
                order.CreatedByUserID = orderDto.CreatedByUserID;

                var existingDetails = order.OrderDetails.ToList();
                foreach (var detail in existingDetails)
                {
                    await _unitOfWork.OrderDetails.DeleteAsync(detail.OrderDetailID);
                }

                decimal totalAmount = 0;
                foreach (var detailDto in orderDto.OrderDetails)
                {
                    var product = await _unitOfWork.Products.GetByExpressionAsync(p => p.ProductID == detailDto.ProductID);
                    if (product == null)
                        throw new InvalidOperationException($"Product with ID {detailDto.ProductID} not found.");

                    var stock = await _unitOfWork.WarehouseStocks.GetByCompositeKeyAsync(orderDto.WarehouseID, detailDto.ProductID);
                    if (stock == null || stock.StockQuantity < detailDto.Quantity)
                        throw new InvalidOperationException($"Insufficient stock for product {product.ProductName}. Available: {stock?.StockQuantity ?? 0}, Requested: {detailDto.Quantity}");

                    var orderDetail = _mapper.Map<OrderDetail>(detailDto);
                    orderDetail.OrderDetailID = Guid.NewGuid();
                    orderDetail.OrderID = order.OrderID;
                    orderDetail.UnitPrice = product.Price;
                    orderDetail.TotalPrice = product.Price * detailDto.Quantity;

                    totalAmount += orderDetail.TotalPrice;
                    await _unitOfWork.OrderDetails.AddAsync(orderDetail);
                }

                order.TotalAmount = totalAmount;
                await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitTransactionAsync();
                _logger.LogInformation("Order edited successfully: {OrderID}.", order.OrderID);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error editing order: {OrderID}, Message: {Message}.", orderId, ex.Message);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);
            if (order == null)
                throw new InvalidOperationException("Order not found.");
            if (order.Status != OrderStatus.Cancelled)
                throw new InvalidOperationException("Only cancelled orders can be deleted.");


            var userRole = await _userLoginService.GetCurrentUserRole();
            var userId = await _userLoginService.GetCurrentUserId();
            var managerWarehouseIds = await _userLoginService.GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

            if (userRole != "Admin" && !managerWarehouseIds.Contains(order.WarehouseID))
            {
                _logger.LogWarning("Unauthorized access to order {OrderID} by user {UserID}.", id, userId);
                throw new UnauthorizedAccessException("You can only view orders for accessible warehouses.");
            }

            foreach (var detail in order.OrderDetails)
            {
                await _unitOfWork.OrderDetails.DeleteAsync(detail.OrderDetailID);
            }
            await _unitOfWork.Orders.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }
    }
}
