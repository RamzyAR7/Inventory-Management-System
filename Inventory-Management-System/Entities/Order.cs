using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Management_System.Entities
{
    public class Order
    {
        public Guid OrderID { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // freign key to User
        public Guid UserID { get; set; }

        // NAVIGATION PROPERTIES
        public ICollection<CustomerOrder> CustomerOrders { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public Shipment Shipment { get; set; }
    }
}
