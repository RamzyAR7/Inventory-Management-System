using System.ComponentModel.DataAnnotations;
using Inventory_Management_System.Entities;

namespace Inventory_Management_System.Models.DTOs.Order
{
    public class OrderReqDto
    {
        public Guid OrderID { get; set; }

        [Required(ErrorMessage = "Customer is required.")]
        public Guid CustomerID { get; set; }

        [Required(ErrorMessage = "Warehouse is required.")]
        public Guid WarehouseID { get; set; }

        [Required(ErrorMessage = "Created by user is required.")]
        public Guid CreatedByUserID { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Range(0, double.MaxValue, ErrorMessage = "Total amount cannot be negative.")]
        public decimal TotalAmount { get; set; }

        [MinLength(1, ErrorMessage = "At least one order detail is required.")]
        public List<OrderDetailReqDto> OrderDetails { get; set; } = new();
    }
}
