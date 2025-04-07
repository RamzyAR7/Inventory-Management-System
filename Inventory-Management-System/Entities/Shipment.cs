using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Entities
{
    public class Shipment
    {
        public Guid ShipmentID { get; set; }
        [Required, MaxLength(50)]
        public ShipmentStatus Status { get; set; }
        public DateTime ShipmentDate { get; set; } = DateTime.UtcNow;
        [MaxLength(100)]
        public string Carrier { get; set; } = "Unknown";

        // forign key to Order
        public Guid OrderID { get; set; }
        // forign key to Warehouse
        public Guid WarehouseID { get; set; }

        // navigation properties

        public Order Order { get; set; }
        public Warehouse Warehouse { get; set; }
    }
}
