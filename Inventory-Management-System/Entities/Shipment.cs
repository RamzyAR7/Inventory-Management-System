using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Entities
{
    public class Shipment
    {
        #region Properties
        public Guid ShipmentID { get; set; }
        [Required, MaxLength(50)]
        public ShipmentStatus Status { get; set; }
        public DateTime ShipmentDate { get; set; } = DateTime.UtcNow;
        [MaxLength(100)]
        public string Carrier { get; set; } = "Unknown";

        // Forign key to Order
        public Guid OrderID { get; set; }
        // Forign key to Warehouse
        public Guid WarehouseID { get; set; }

        // Navigation properties

        public Order Order { get; set; }
        public Warehouse Warehouse { get; set; }
        #endregion
    }
}
