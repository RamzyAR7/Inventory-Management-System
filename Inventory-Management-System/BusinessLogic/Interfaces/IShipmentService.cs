using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Shipment;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface IShipmentService
    {
        Task<(IEnumerable<Shipment> Items, int TotalCount)> GetPagedShipmentsAsync(int pageNumber, int pageSize);
        Task<Shipment> GetShipmentByIdAsync(Guid shipmentId);
        Task UpdateShipmentAsync(ShipmentReqDto dto);
        Task DeleteShipmentAsync(Guid shipmentId);
    }
}
