using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Entities
{
    public class Warehouse
    {
        #region Properties
        public Guid WarehouseID { get; set; }
        [Required, MaxLength(100)]
        public string WarehouseName { get; set; }
        [Required, MaxLength(255)]
        public string Address { get; set; }
        // Foreign key to User
        public Guid ManagerID { get; set; }

        // Navigation properties
        public User Manager { get; set; }
        public ICollection<WarehouseStock> WarehouseStocks { get; set; }
        public ICollection<Shipment> Shipments { get; set; }
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; }
        #endregion
    }
}
