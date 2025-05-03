using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS.Data.Entities
{
    public class OrderDetail
    {
        [Key]
        public Guid OrderDetailID { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }
        // Foreign Keys
        [Required]
        public Guid OrderID { get; set; }

        [Required]
        public Guid ProductID { get; set; }

        // Navigation Properties
        public Order Order { get; set; }

        public Product Product { get; set; }
    }
}
