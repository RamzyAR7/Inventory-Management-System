namespace Inventory_Management_System.Entities
{
    public class DeliveryMan
    {
        public Guid DeliveryManID { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DeliveryManStatus Status { get; set; } // "Free" or "Busy"
        public Guid? ManagerID { get; set; } // New property to link to a manager

        // Navigation properties
        public User Manager { get; set; } // Assuming User entity represents managers
        public ICollection<Shipment> Shipments { get; set; }
    }
}
