using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.User;

namespace Inventory_Management_System.Models.DTOs.Warehouse
{
    public class WarehouseResDto
    {
        public Guid WarehouseID { get; set; }
        public string Address { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public Guid ManagerID { get; set; }
        public UserResDto? Manager { get; set; }

        public ICollection<WarehouseStock> WarehouseStocks { get; set; } = new List<WarehouseStock>();
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
        public ICollection<Entities.InventoryTransaction> InventoryTransactions { get; set; }
    }
}
