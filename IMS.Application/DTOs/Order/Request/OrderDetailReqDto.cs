using System.ComponentModel.DataAnnotations;

namespace IMS.Application.DTOs.Order.Request
{
    public class OrderDetailReqDto
    {
        [Required(ErrorMessage = "Product is required.")]
        public Guid ProductID { get; set; }

        public string ProductName { get; set; } = string.Empty; // Add this

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative.")]
        public decimal UnitPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total price cannot be negative.")]
        public decimal TotalPrice { get; set; }
    }
}
