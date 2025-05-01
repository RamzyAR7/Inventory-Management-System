using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Management_System.Entities
{
    public class Order
    {
        [Key]
        public Guid OrderID { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Foreign key to User
        [Required]
        public Guid CreatedByUserID { get; set; }

        // Navigation property to User
        public User CreatedByUser { get; set; } = null!;

        // Navigation properties

        public ICollection<OrderDetail> OrderDetails { get; set; }
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; }
        public Customer Customer { get; set; }
        [Required]
        public Guid CustomerID { get; set; }
        public Shipment Shipment { get; set; }

        public Guid WarehouseID { get; set; }
        public Warehouse? Warehouse { get; set; }
    }
}
