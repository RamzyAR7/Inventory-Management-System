using IMS.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace IMS.BLL.DTOs.Shipment
{
    public class ShipmentReqDto
    {
        [Required]
        public Guid ShipmentID { get; set; }

        [Required]
        [StringLength(200)]
        public string Destination { get; set; }

        [Required]
        public ShipmentStatus Status { get; set; }

        [Required]
        public DeliveryMethod DeliveryMethod { get; set; } // Delivery method (e.g., Delivering, Pickup)

        [Required]
        [Range(1, int.MaxValue)]
        public int ItemCount { get; set; }

        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public Guid? DeliveryManID { get; set; }
        [Required]
        public Guid OrderID { get; set; }
    }
}
