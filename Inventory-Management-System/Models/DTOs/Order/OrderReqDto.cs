using Inventory_Management_System.Entities;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.Order
{
    public class OrderReqDto
    {
        //[Required]
        //public Guid CustomerID { get; set; }

        [Required]
        public Guid CreatedByUserID { get; set; }

        public List<Guid> CustomerID { get; set; } = new List<Guid>(); 

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
    }
}
