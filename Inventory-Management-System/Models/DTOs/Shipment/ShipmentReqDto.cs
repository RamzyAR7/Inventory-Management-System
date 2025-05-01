using Inventory_Management_System.Entities;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.Shipment
{
    public class ShipmentReqDto
    {
        [Required]
        public Guid ShipmentID { get; set; }

        [Required]
        [StringLength(50)]
        public string TrackingNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string Destination { get; set; }

        [Required]
        public ShipmentStatus Status { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ItemCount { get; set; }

        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        [Required]
        public Guid OrderID { get; set; }
    }
}
