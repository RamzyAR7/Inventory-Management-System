using System.ComponentModel.DataAnnotations;

namespace IMS.Domain.Entities
{
    public class User
    {
        [Key]
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
        public bool IsActive { get; set; } = true;
        public string Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Self-reference for Manager
        public Guid? ManagerID { get; set; }
        public User? Manager { get; set; }

        // Navigation properties
        public ICollection<Warehouse> ManagedWarehouses { get; set; }
        public ICollection<DeliveryMan> DeliveryMen { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
