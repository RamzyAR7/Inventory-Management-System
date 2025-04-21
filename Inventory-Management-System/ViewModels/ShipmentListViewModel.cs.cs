namespace Inventory_Management_System.ViewModels
{
    public class ShipmentListViewModel
    {
        public Guid ShipmentID { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

    }
}
