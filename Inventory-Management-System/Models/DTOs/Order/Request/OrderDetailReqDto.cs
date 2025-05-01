using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.Order
{
    public class OrderDetailReqDto
    {
        [Required(ErrorMessage = "Product is required.")]
        public Guid ProductID { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative.")]
        public decimal UnitPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total price cannot be negative.")]
        public decimal TotalPrice { get; set; }
    }
}
