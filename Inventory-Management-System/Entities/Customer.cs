using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Entities
{
    public class Customer
    {
        public Guid CustomerID { get; set; }
        [Required, MaxLength(100)]
        public string FullName { get; set; }
        [Required, MaxLength(100)]
        public string PhoneNumber { get; set; }
        [MaxLength(255)]
        public string Email { get; set; }
        [Required, MaxLength(15)]
        public string Address { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // navigation properties
        public ICollection<CustomerOrder> CustomerOrders { get; set; }
    }
}
