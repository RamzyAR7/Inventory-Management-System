using Inventory_Management_System.Entities;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.Order
{
    public class OrderReqDto
    {

        public List<Guid> CustomerID { get; set; } = new List<Guid>();

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
    }
}