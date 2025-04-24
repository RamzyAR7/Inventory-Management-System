using Inventory_Management_System.Entities;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.Order
{
    public class OrderResDto
    {
        [Key]
        public Guid OrderID { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }

        public Guid CreatedByUserID { get; set; }
        public string CreatedByUserName { get; set; }

        public Guid CustomerID { get; set; }
        public string CustomerName { get; set; }

        public List<CustomerOrderResDto> CustomerOrders { get; set; } = new List<CustomerOrderResDto>();
    }
}
