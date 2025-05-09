using IMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.BLL.Models
{
    public class OrderRecipeViewModel
    {
        public Guid OrderID { get; set; }
        public DateTime OrderDate { get; set; }
         public string Status { get; set; }
        public string CustomerName { get; set; }
        public string WarehouseName { get; set; }
        public decimal TotalAmount { get; set; }
         public List<OrderDetailViewModel> OrderDetails { get; set; }
    }

     public class OrderDetailViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

}
