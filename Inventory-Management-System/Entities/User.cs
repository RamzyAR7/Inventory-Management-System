using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Entities
{
    public class User
    {
        [Key]
        public Guid UserID { get; set; }
<<<<<<< HEAD
        public string UserName { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
        public string Role { get; set; }
=======

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string HashedPassword { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

>>>>>>> 01737cbb83fa284d4ed4454ab7328f8f3dcc0f59
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Self-reference for Manager
        public Guid? ManagerID { get; set; }
        public User? Manager { get; set; }

        // Navigation properties
        public ICollection<Warehouse> ManagedWarehouses { get; set; } = new List<Warehouse>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
