using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Management_System.Entities
{
    public class InventoryTransaction
    {
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

        //navigation properties
        public Product Product { get; set; }
        public Warehouse Warehouse { get; set; }
    }
}
