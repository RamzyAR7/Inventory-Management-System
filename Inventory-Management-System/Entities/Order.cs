using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Management_System.Entities
{
    public class Order
    {
        #region Properties
        public Guid OrderID { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(10,2)")]
        [NotMapped]
        public decimal TotalAmount => OrderDetails?.Sum(od => od.TotalPrice) ?? 0;
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        
        // Foreign key to User
        public Guid CreatedByUserID { get; set; }

        // Navigation properties
        public User CreatedByUser { get; set; }
        public ICollection<CustomerOrder> CustomerOrders { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public Shipment Shipment { get; set; }
        #endregion
    }
}
