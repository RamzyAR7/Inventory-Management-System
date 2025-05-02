namespace Inventory_Management_System.Entities
{

    public class Shipment
    {
        public Guid ShipmentID { get; set; }   // Primary Key
        public string? TrackingNumber { get; set; } = null!; // Tracking number for the shipment
        public string? Destination { get; set; } = null!; // Destination address for the shipment
        public ShipmentStatus Status { get; set; }
        public int ItemCount { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        // Relationships
        public Guid OrderID { get; set; }
        public Order? Order { get; set; }
    }
}