using System.ComponentModel.DataAnnotations;

namespace IMS.Data.Entities
{

    public class Shipment
    {
        public Guid ShipmentID { get; set; }   // Primary Key
        public string? Destination { get; set; } = null!; // Destination address for the shipment
        public ShipmentStatus Status { get; set; }
        public int ItemCount { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DeliveryMethod DeliveryMethod { get; set; } // Delivery method (e.g., Delivering, Pickup)

        // Relationships
        public Guid OrderID { get; set; }
        public Order? Order { get; set; }
        public Guid? DeliveryManID { get; set; }
        public DeliveryMan? DeliveryMan { get; set; }
        [Required]
        public string DeliveryName { get; set; }
        [Required]
        public string DeliveryPhoneNumber { get; set; }
    }
}