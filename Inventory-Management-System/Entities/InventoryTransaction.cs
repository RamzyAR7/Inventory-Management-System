using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Management_System.Entities
{
    public class InventoryTransaction
    {
        #region Properties
        public Guid TransactionID { get; set; }
        [Required]
        public TransactionType Type { get; set; }
        [Required]
        public long Quantity { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        // Foreign key to Product
        public Guid ProductID { get; set; }
        // Foreign key to Warehouse
        public Guid WarehouseID { get; set; }

        // Navigation properties
        public Product Product { get; set; }
        public Warehouse Warehouse { get; set; }
        #endregion
    }
}
