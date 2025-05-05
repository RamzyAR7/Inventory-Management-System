using IMS.DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace IMS.BAL.DTOs.Order.Responce
{
    public class OrderResponseDto
    {
        public Guid OrderID { get; set; }

        [Required]
        public Guid CustomerID { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        public Guid WarehouseID { get; set; }

        [Required]
        public string WarehouseName { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }

        [Required]
        public Guid CreatedByUserID { get; set; }

        [Required]
        public string CreatedByUserName { get; set; }

        public DateTime OrderDate { get; set; }
    }
}
