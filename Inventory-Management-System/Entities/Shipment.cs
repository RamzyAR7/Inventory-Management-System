namespace Inventory_Management_System.Entities
{

    public class Shipment
    {
        public Guid ShipmentID { get; set; }   // Primary Key
        public string TrackingNumber { get; set; }
        public string Destination { get; set; }
        public string Status { get; set; }
        public int ItemCount { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        // Relationships
        public Guid OrderID { get; set; }
        public Order? Order { get; set; }

        public Guid WarehouseID { get; set; }
        public Warehouse? Warehouse { get; set; }
    }
}