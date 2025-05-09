using IMS.Application.DTOs.Shipment;
using IMS.Domain.Entities;
using IMS.Domain.Enums;

namespace IMS.Application.Services.Interface
{
    public interface IShipmentService
    {
        Task<(IEnumerable<Shipment> Items, int TotalCount)> GetPagedShipmentsAsync(
                    int pageNumber,
                    int pageSize,
                    ShipmentStatus? statusFilter = null,
                    string sortBy = "OrderDate",
                    bool sortDescending = false);
        Task<Shipment> GetShipmentByIdAsync(Guid shipmentId);
        Task UpdateShipmentStatusAsync(Guid shipmentId, ShipmentStatus newStatus);
        Task UpdateDeliveryMethoud(ShipmentReqDto shipmentDto);
        Task DeleteShipmentAsync(Guid shipmentId);
    }
}
