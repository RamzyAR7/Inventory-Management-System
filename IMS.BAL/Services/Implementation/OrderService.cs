using AutoMapper;
using IMS.BAL.DTOs.Order.Request;
using IMS.BAL.DTOs.Order.Responce;
using IMS.Data.Entities;
using IMS.Data.UnitOfWork;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        public async Task<(IEnumerable<OrderResponseDto> Items, int TotalCount)> GetPagedOrdersAsync(int pageNumber, int pageSize, OrderStatus? statusFilter = null)
        {
            try
            {
                var userRole = GetCurrentUserRole();
                var userId = GetCurrentUserId();
                var managerWarehouseIds = await GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

                if (!managerWarehouseIds.Any() && userRole != "Admin")
                {
                    _logger.LogWarning("No warehouses accessible for user {UserId} with role {Role}.", userId, userRole);
                    return (Enumerable.Empty<OrderResponseDto>(), 0);
                }

                var includes = new Expression<Func<Order, object>>[]
                {
                    o => o.Customer,
                    o => o.Warehouse
                };

                Expression<Func<Order, bool>> predicate = null;
                if (statusFilter.HasValue)
                {
                    predicate = o => o.Status == statusFilter.Value;
                }
                else if (userRole != "Admin")
                {
                    predicate = o => managerWarehouseIds.Contains(o.WarehouseID);
                }

                var (orders, totalCount) = await _unitOfWork.Orders.GetPagedAsync(pageNumber, pageSize, predicate, includes);
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

        public async Task<OrderDetailResponseDto?> GetByIdAsync(Guid id)
        {
            var order = await _unitOfWork.Orders.GetByIdWithDetailsAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Order not found: {OrderID}.", id);
                return null;
            }

            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();
            var managerWarehouseIds = await GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

            if (userRole != "Admin" && !managerWarehouseIds.Contains(order.WarehouseID))
            {
                _logger.LogWarning("Unauthorized access to order {OrderID} by user {UserID}.", id, userId);
                throw new UnauthorizedAccessException("You can only view orders for accessible warehouses.");
            }

            return _mapper.Map<OrderDetailResponseDto>(order);
        }

        public async Task CreateAsync(OrderReqDto orderDto, Guid userId)
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
                var order = _mapper.Map<Order>(orderDto);
                order.CreatedByUserID = userId;

                // Clear OrderDetails to avoid duplicates from AutoMapper
                order.OrderDetails = new List<OrderDetail>();

                // Validate warehouse access
                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserID == order.CreatedByUserID);
                if (user == null)
                    throw new InvalidOperationException("User not found.");

                var accessibleWarehouseIds = await GetAccessibleWarehouseIdsAsync(user.Role, order.CreatedByUserID);
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
                    var (isValid, errorMessage, orderDetail) = await ValidateAndAddProductAsync(
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
            var order = await _unitOfWork.Orders.GetByIdAsync(o => o.OrderID == orderId, o => o.OrderDetails, o => o.Customer);
            if (order == null)
                throw new InvalidOperationException("Order not found.");

            await ValidateUserAccessAsync(order.WarehouseID);

            if (!IsValidStatusTransition(order.Status, newStatus))
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
                        var shipment = await _unitOfWork.Shipments.GetByIdAsync(s => s.OrderID == orderId);
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

                    //order.TotalAmount = 0;
                }

                order.Status = newStatus;
                await _unitOfWork.Orders.UpdateAsync(order);
                await _unitOfWork.SaveAsync();
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Order status updated: {OrderID}, New Status: {Status}.", orderId, newStatus);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error updating order status: {OrderID}, Message: {Message}.", orderId, ex.Message);
                throw;
            }
        }

        public async Task EditAsync(Guid orderId, OrderReqDto orderDto)
        {
            if (orderId != orderDto.OrderID)
                throw new InvalidOperationException("Order ID mismatch.");

            await ValidateUserAccessAsync(orderDto.WarehouseID);
            await ValidateOrderDtoAsync(orderDto);

            var order = await _unitOfWork.Orders.GetByIdAsync(o => o.OrderID == orderId, o => o.OrderDetails);
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
                    var product = await _unitOfWork.Products.GetByIdAsync(p => p.ProductID == detailDto.ProductID);
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
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("Order edited successfully: {OrderID}.", order.OrderID);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
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


            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();
            var managerWarehouseIds = await GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

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
        public async Task<(bool isValid, string errorMessage, OrderDetail orderDetail)> ValidateAndAddProductAsync(Guid warehouseId, Guid productId, int quantity, Guid userId)
        {
            try
            {
                // Validate warehouse access
                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserID == userId);
                if (user == null)
                    return (false, "User not found.", null);

                var accessibleWarehouseIds = await GetAccessibleWarehouseIdsAsync(user.Role, userId);
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

        public async Task<List<Product>> GetProductsByWarehouseAndCategoryAsync(Guid warehouseId, Guid? categoryId)
        {
            try
            {
                // Validate warehouse access
                var userRole = GetCurrentUserRole();
                var userId = Guid.Parse(GetCurrentUserId());
                var accessibleWarehouseIds = await GetAccessibleWarehouseIdsAsync(userRole, userId);

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

                return products.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products for WarehouseID {WarehouseID}, CategoryID {CategoryID}.", warehouseId, categoryId);
                throw;
            }
        }

        private async Task ValidateOrderDtoAsync(OrderReqDto orderDto)
        {
            if (orderDto.OrderDetails == null || !orderDto.OrderDetails.Any())
                throw new InvalidOperationException("At least one order detail is required.");

            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(w => w.WarehouseID == orderDto.WarehouseID);
            if (warehouse == null)
                throw new InvalidOperationException("Warehouse not found.");

            var customer = await _unitOfWork.Customers.GetByIdAsync(c => c.CustomerID == orderDto.CustomerID);
            if (customer == null)
                throw new InvalidOperationException("Customer not found.");

            var productIds = orderDto.OrderDetails.Select(d => d.ProductID).Distinct().ToList();
            var products = await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.ProductID));
            if (products.Count() != productIds.Count)
                throw new InvalidOperationException("One or more products not found.");
        }

        private async Task ValidateUserAccessAsync(Guid warehouseId)
        {
            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();
            var managerWarehouseIds = await GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

            if (userRole != "Admin" && !managerWarehouseIds.Contains(warehouseId))
                throw new UnauthorizedAccessException("You can only access your assigned warehouses.");
        }

        public async Task<List<Guid>> GetAccessibleWarehouseIdsAsync(string role, Guid userId)
        {
            if (role == "Admin")
                return (await _unitOfWork.Warehouses.GetAllAsync()).Select(w => w.WarehouseID).ToList();

            if (role == "Manager")
            {
                var warehouses = await _unitOfWork.Warehouses.FindAsync(w => w.ManagerID == userId);
                return warehouses.Select(w => w.WarehouseID).ToList();
            }

            if (role == "Employee")
            {
                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserID == userId);
                if (user == null)
                    throw new InvalidOperationException("User not found.");

                var manager = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Role == "Manager");
                if (manager == null)
                    throw new InvalidOperationException("No manager found for this employee.");

                var warehouses = await _unitOfWork.Warehouses.FindAsync(w => w.ManagerID == manager.UserID);
                return warehouses.Select(w => w.WarehouseID).ToList();
            }

            return new List<Guid>();
        }

        private bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                (OrderStatus.Pending, OrderStatus.Confirmed) => true,
                (OrderStatus.Pending, OrderStatus.Cancelled) => true,
                (OrderStatus.Confirmed, OrderStatus.Pending) => true,
                (OrderStatus.Confirmed, OrderStatus.Shipped) => true,
                (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
                (OrderStatus.Shipped, OrderStatus.Delivered) => true,
                (OrderStatus.Shipped, OrderStatus.Cancelled) => true,
                (OrderStatus.Shipped, OrderStatus.Pending) => true,
                (OrderStatus.Cancelled, OrderStatus.Pending) => true,
                _ => false
            };
        }

        private string GetCurrentUserRole()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("User not authenticated.");

            var user = _unitOfWork.Users.GetByIdAsync(e => e.UserID == Guid.Parse(userId)).Result;
            if (user == null)
                throw new InvalidOperationException("User not found.");

            return user.Role;
        }

        private string GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("User not authenticated.");

            return userId;
        }
    }
}
