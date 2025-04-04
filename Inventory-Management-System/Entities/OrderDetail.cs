using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Management_System.Entities
{
    public class OrderDetail
    {
        public Guid OrderDetailID { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        //freign key to Order
        public Guid OrderID { get; set; }
        public Guid ProductID { get; set; }

        // NAVIGATION PROPERTIES
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
