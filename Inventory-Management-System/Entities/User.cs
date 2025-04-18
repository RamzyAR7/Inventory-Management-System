using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Entities
{
    public class User
    {
        #region Properties
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // self relationship for manager
        public Guid? ManagerID { get; set; }
        public User Manager { get; set; }

        // Navigation properties
        public ICollection<Warehouse> ManagedWarehouses { get; set; }
        public ICollection<Order> Orders { get; set; }
        #endregion
    }
}
