using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Management_System.Entities
{
    public class OrderDetail
    {
        #region Properties
        public Guid OrderDetailID { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        [NotMapped]
        public decimal TotalPrice => Quantity * UnitPrice;
        // Foreign key to Order
        public Guid OrderID { get; set; }
        public Guid ProductID { get; set; }

        // Navigation properties
        public Order Order { get; set; }
        public Product Product { get; set; }
        #endregion
    }
}
