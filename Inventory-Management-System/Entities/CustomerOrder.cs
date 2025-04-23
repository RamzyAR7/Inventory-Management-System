using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Entities
{
    public class CustomerOrder
    {
        [Key]
        public Guid CustomerOrderID { get; set; }

        [Required]
        public string CustomerName { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        // Foreign Keys
        [Required]
        public Guid CustomerID { get; set; }

        [Required]
        public Guid OrderID { get; set; }

        // Navigation Properties
        public Customer Customer { get; set; }
        public Order Order { get; set; }
    }
}
