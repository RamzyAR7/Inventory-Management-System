using IMS.BAL.DTOs.Shipment;
using IMS.Data.Entities;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface IShipmentService
    {
        Task<(IEnumerable<Shipment> Items, int TotalCount)> GetPagedShipmentsAsync(int pageNumber, int pageSize, ShipmentStatus? statusFilter = null);
        Task<Shipment> GetShipmentByIdAsync(Guid shipmentId);
        Task UpdateShipmentStatusAsync(Guid shipmentId, ShipmentStatus newStatus);
        Task UpdateDeliveryMethoud(ShipmentReqDto shipmentDto);
        Task DeleteShipmentAsync(Guid shipmentId);
    }
}
