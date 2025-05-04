using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Microsoft.EntityFrameworkCore;
using IMS.BAL.DTOs.Shipment;
using IMS.Data.UnitOfWork;
using IMS.Data.Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using IMS.BAL.DTOs.Order.Responce;

namespace Inventory_Management_System.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ShipmentService> _logger;

        public ShipmentService(IUnitOfWork unitOfWork, IOrderService orderService, IMapper mapper, ILogger<ShipmentService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _orderService = orderService;
            _mapper = mapper;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(IEnumerable<Shipment> Items, int TotalCount)> GetPagedShipmentsAsync(int pageNumber, int pageSize, ShipmentStatus? statusFilter = null)
        {
            try
            {
                var userRole = GetCurrentUserRole();
                var userId = GetCurrentUserId();
                var managerWarehouseIds = await GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

                var includes = new Expression<Func<Shipment, object>>[]
                {
                    s => s.Order,
                    s => s.Order.Customer,
                    s => s.Order.Warehouse
                };

                // Build the predicate based on user role and status filter
                Expression<Func<Shipment, bool>> predicate = null;

                if (userRole != "Admin")
                {
                    // For non-admin users, they can only see shipments from their accessible warehouses
                    predicate = s => managerWarehouseIds.Contains(s.Order.WarehouseID);
                }

                // Apply additional status filter if provided
                if (statusFilter.HasValue)
                {
                    if (predicate == null)
                    {
                        predicate = s => s.Status == statusFilter.Value;
                    }
                    else
                    {
                        var originalPredicate = predicate;
                        predicate = s => originalPredicate.Compile()(s) && s.Status == statusFilter.Value;
                    }
                }

                var (items, totalCount) = await _unitOfWork.Shipments.GetPagedAsync(pageNumber, pageSize, predicate, includes);
                _logger.LogInformation("GetPagedShipmentsAsync - Retrieved {ItemCount} shipments, TotalCount: {TotalCount}", items.Count(), totalCount);
                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPagedShipmentsAsync - Error retrieving shipments: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Shipment> GetShipmentByIdAsync(Guid shipmentId)
        {
            try
            {
                var includes = new Expression<Func<Shipment, object>>[]
                {
                    s => s.Order,
                    s => s.Order.Customer,
                    s => s.Order.Warehouse,
                    s => s.DeliveryMan
                };

                var shipment = await ValidateUserAccessAsync(shipmentId, includes);
                if (shipment == null)
                {
                    _logger.LogWarning("GetShipmentByIdAsync - Shipment not found for ShipmentID: {ShipmentID}", shipmentId);
                    throw new KeyNotFoundException("Shipment not found.");
                }
                _logger.LogInformation("GetShipmentByIdAsync - Retrieved shipment for ShipmentID: {ShipmentID}", shipmentId);
                return shipment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetShipmentByIdAsync - Error retrieving shipment: {Message}", ex.Message);
                throw;
            }
        }

        public async Task UpdateShipmentStatusAsync(Guid shipmentId, ShipmentStatus newStatus)
        {
            try
            {
                var shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentId,
                    includes: new Expression<Func<Shipment, object>>[] { s => s.Order });

                if (shipment == null)
                {
                    throw new KeyNotFoundException("Shipment not found.");
                }

                // Handle status transitions
                switch (newStatus)
                {
                    case ShipmentStatus.InTransit:
                        shipment.Status = newStatus;
                        shipment.ShippedDate = DateTime.UtcNow;
                        break;


                    case ShipmentStatus.Cancelled:
                        shipment.Status = newStatus;
                        if (shipment.Order != null)
                        {
                            var transactions = await _unitOfWork.InventoryTransactions.FindAsync(t => t.OrderID == shipment.OrderID && t.Type == TransactionType.Out);
                            foreach (var trans in transactions)
                            {
                                var stock = await _unitOfWork.WarehouseStocks.GetByCompositeKeyAsync(shipment.Order.WarehouseID, trans.ProductID);
                                if (stock != null)
                                {
                                    stock.StockQuantity += trans.Quantity;
                                    await _unitOfWork.WarehouseStocks.UpdateAsync(stock);
                                }
                                await _unitOfWork.InventoryTransactions.DeleteAsync(trans.TransactionID);
                            }
                            shipment.Order.Status = OrderStatus.Pending;
                            await _unitOfWork.Orders.UpdateAsync(shipment.Order);
                        }
                        break;

                    case ShipmentStatus.Delivered:
                        shipment.Status = newStatus;
                        shipment.DeliveryDate = DateTime.UtcNow;
                        if (shipment.Order != null)
                        {
                            shipment.Order.Status = OrderStatus.Delivered;
                            await _unitOfWork.Orders.UpdateAsync(shipment.Order);
                        }
                        if(shipment.DeliveryMan != null)
                        {
                            shipment.DeliveryMan.Status = DeliveryManStatus.Free;
                            await _unitOfWork.DeliveryMen.UpdateAsync(shipment.DeliveryMan);
                        }
                        break;

                    default:
                        shipment.Status = newStatus;
                        break;
                }

                await _unitOfWork.Shipments.UpdateAsync(shipment);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating shipment status for ID: {shipmentId}");
                throw;
            }
        }

        public async Task UpdateDeliveryMethoud(ShipmentReqDto shipmentDto)
        {
            if (shipmentDto.Status != ShipmentStatus.Pending)
            {
                throw new InvalidOperationException("Shipment must be in Pending status to update delivery method.");
            }

            var shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentDto.ShipmentID, includes: new Expression<Func<Shipment, object>>[] { s => s.DeliveryMan });
            if (shipment == null)
            {
                throw new KeyNotFoundException("Shipment not found.");
            }

            shipment.DeliveryMethod = shipmentDto.DeliveryMethod;

            if (shipmentDto.DeliveryMethod == DeliveryMethod.Delivering)
            {
                if (!shipmentDto.DeliveryManID.HasValue)
                {
                    throw new InvalidOperationException("Delivery man must be selected for Delivering method.");
                }

                var deliveryMan = await _unitOfWork.DeliveryMen.GetByIdAsync(shipmentDto.DeliveryManID.Value);
                if (deliveryMan == null || !deliveryMan.IsActive || deliveryMan.Status != DeliveryManStatus.Free)
                {
                    throw new InvalidOperationException("Selected delivery man is not available.");
                }

                shipment.DeliveryManID = shipmentDto.DeliveryManID;
                shipment.DeliveryName = deliveryMan.FullName; // Set from DeliveryMan
                shipment.DeliveryPhoneNumber = deliveryMan.PhoneNumber; // Set from DeliveryMan
                deliveryMan.Status = DeliveryManStatus.Busy;
                await _unitOfWork.DeliveryMen.UpdateAsync(deliveryMan);
            }
            else
            {
                shipment.DeliveryManID = null;
            }

            shipment.Status = ShipmentStatus.InTransit;
            await _unitOfWork.Shipments.UpdateAsync(shipment);
            await _unitOfWork.SaveAsync();
        }
        public async Task DeleteShipmentAsync(Guid shipmentId)
        {
            try
            {
                var shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentId);
                if (shipment == null)
                {
                    _logger.LogWarning("DeleteShipmentAsync - Shipment not found for ShipmentID: {ShipmentID}", shipmentId);
                    throw new KeyNotFoundException("Shipment not found.");
                }

                if (shipment.Status != ShipmentStatus.Cancelled)
                {
                    _logger.LogWarning("DeleteShipmentAsync - Cannot delete shipment with ShipmentID: {ShipmentID} because status is {Status}, not Cancelled", shipmentId, shipment.Status);
                    throw new InvalidOperationException("Only cancelled shipments can be deleted.");
                }

                await _unitOfWork.Shipments.DeleteAsync(shipment.ShipmentID);
                await _unitOfWork.SaveAsync();
                _logger.LogInformation("DeleteShipmentAsync - Successfully deleted shipment for ShipmentID: {ShipmentID}", shipmentId);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteShipmentAsync - Error deleting shipment: {Message}", ex.Message);
                throw;
            }
        }
        public async Task<List<Guid>> GetAccessibleWarehouseIdsAsync(string role, Guid userId)
        {
            if (role == "Admin")
            {
                return (await _unitOfWork.Warehouses.GetAllAsync()).Select(w => w.WarehouseID).ToList();
            }

            if (role == "Manager")
            {
                var warehouses = await _unitOfWork.Warehouses.FindAsync(w => w.ManagerID == userId);
                return warehouses.Select(w => w.WarehouseID).ToList();
            }

            if (role == "Employee")
            {
                // Get the employee's manager
                var employee = await _unitOfWork.Users.GetByIdAsync(userId);
                if (employee == null)
                    throw new InvalidOperationException("User not found.");

                // If employee has a direct manager assigned
                if (employee.ManagerID.HasValue)
                {
                    var managerWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.ManagerID == employee.ManagerID.Value);
                    return managerWarehouses.Select(w => w.WarehouseID).ToList();
                }

                // Fallback: Get all managers' warehouses if no direct manager assigned
                var managers = await _unitOfWork.Users.FindAsync(u => u.Role == "Manager");
                var allManagerWarehouses = new List<Warehouse>();

                foreach (var manager in managers)
                {
                    var warehouses = await _unitOfWork.Warehouses.FindAsync(w => w.ManagerID == manager.UserID);
                    allManagerWarehouses.AddRange(warehouses);
                }

                return allManagerWarehouses.Select(w => w.WarehouseID).Distinct().ToList();
            }

            return new List<Guid>();
        }
        private async Task<Shipment> ValidateUserAccessAsync(Guid shipmentId, Expression<Func<Shipment, object>>[]? includes = null)
        {
            Shipment shipment;

            if (includes == null)
            {
                shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentId);
            }
            else
            {
                shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentId, includes);
            }
            if (shipment == null)
            {
                _logger.LogWarning("GetShipmentByIdAsync - Shipment not found for ShipmentID: {ShipmentID}", shipmentId);
                throw new KeyNotFoundException("Shipment not found.");
            }

            var userRole = GetCurrentUserRole();
            var userId = GetCurrentUserId();
            var managerWarehouseIds = await GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

            if (userRole != "Admin" && !managerWarehouseIds.Contains(shipment.Order.WarehouseID))
            {
                _logger.LogWarning("Unauthorized access to shipment {ShipmentID} by user {UserID}.", shipmentId, userId);
                throw new UnauthorizedAccessException("You can only view shipment for accessible warehouses.");
            }
            _logger.LogInformation("GetShipmentByIdAsync - Retrieved shipment for ShipmentID: {ShipmentID}", shipmentId);
            return shipment;
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
