using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Entities
{
    public class User
    {
        #region Properties
        public Guid UserID { get; set; }
        [Required , MaxLength(100)]
        public string FullName { get; set; }
        [Required, MaxLength(100)]
        public string Email { get; set; }
        [Required, MaxLength(255)]
        public string PasswordHash { get; set; }
        [Required]
        public UserRole Role { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Warehouse> ManagedWarehouses { get; set; }
        public ICollection<Order> Orders { get; set; }
        #endregion
    }
}
