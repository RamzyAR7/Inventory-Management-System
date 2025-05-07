using IMS.BLL.Services.Interface;
using IMS.BLL.SharedServices.Interface;
using IMS.DAL.Entities;
using IMS.DAL.UnitOfWork;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace IMS.BLL.SharedServices.Impelimentation
{
    public class ShipmentHelperService: IShipmentHelperService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ShipmentHelperService> _logger;
        private readonly IWhoIsUserLoginService _userLoginService;
        private readonly IDeliveryManService _deliveryManService;

        public ShipmentHelperService(IUnitOfWork unitOfWork, ILogger<ShipmentHelperService> logger, IWhoIsUserLoginService userLoginService, IDeliveryManService deliveryManService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userLoginService = userLoginService;
            _deliveryManService = deliveryManService;
        }

        public async Task<Shipment> ValidateUserAccessShipmentAsync(Guid shipmentId, Expression<Func<Shipment, object>>[]? includes = null)
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

            var userRole = await _userLoginService.GetCurrentUserRole();
            var userId = await _userLoginService.GetCurrentUserId();
            var managerWarehouseIds = await _userLoginService.GetAccessibleWarehouseIdsAsync(userRole, Guid.Parse(userId));

            if (userRole != "Admin" && !managerWarehouseIds.Contains(shipment.Order.WarehouseID))
            {
                _logger.LogWarning("Unauthorized access to shipment {ShipmentID} by user {UserID}.", shipmentId, userId);
                throw new UnauthorizedAccessException("You can only view shipment for accessible warehouses.");
            }
            _logger.LogInformation("GetShipmentByIdAsync - Retrieved shipment for ShipmentID: {ShipmentID}", shipmentId);
            return shipment;
        }

        public async Task<IEnumerable<DeliveryMan>> GetAllFreeDeliveryMen()
        {
            var userId = Guid.Parse(await _userLoginService.GetCurrentUserId());

            var freeDeliveryMen = await _deliveryManService.GetAllAsync();
            freeDeliveryMen = freeDeliveryMen.Where(d => d.IsActive && d.Status == DeliveryManStatus.Free && d.ManagerID == userId).ToList();
            return freeDeliveryMen;
        }
    }
}
