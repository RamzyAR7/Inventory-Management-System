using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Entities
{
    public class User
    {
        [Key]
        public Guid UserID { get; set; }

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string HashedPassword { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Self-reference for Manager
        public Guid? ManagerID { get; set; }
        public User? Manager { get; set; }

        // Navigation properties
        public ICollection<Warehouse> ManagedWarehouses { get; set; } = new List<Warehouse>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
