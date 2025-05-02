using AutoMapper;
using Inventory_Management_System.Entities;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Models.DTOs.Shipment;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_System.Services
{
    public class ShipmentService : IShipmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly ILogger<ShipmentService> _logger;

        public ShipmentService(IUnitOfWork unitOfWork, IOrderService orderService, IMapper mapper, ILogger<ShipmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _orderService = orderService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<(IEnumerable<Shipment> Items, int TotalCount)> GetPagedShipmentsAsync(int pageNumber, int pageSize, ShipmentStatus? statusFilter = null)
        {
            try
            {
                var includes = new Expression<Func<Shipment, object>>[]
                {
                    s => s.Order,
                    s => s.Order.Customer,
                    s => s.Order.Warehouse
                };

                // Remove the OrderStatus.Shipped filter and add optional status filter
                Expression<Func<Shipment, bool>> predicate = null;
                if (statusFilter.HasValue)
                {
                    predicate = s => s.Status == statusFilter.Value;
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
        // In your ShipmentService class

        public async Task<Shipment> GetShipmentByIdAsync(Guid shipmentId)
        {
            try
            {
                var includes = new Expression<Func<Shipment, object>>[]
                {
                    s => s.Order,
                    s => s.Order.Customer,
                    s => s.Order.Warehouse
                };

                var shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentId, includes);
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
                        // Just update the status, no other changes needed
                        shipment.Status = newStatus;
                        shipment.ShippedDate = DateTime.UtcNow;
                        break;

                    case ShipmentStatus.Cancelled:
                        shipment.Status = newStatus;
                        if (shipment.Order != null)
                        {
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
    }

}
