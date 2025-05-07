using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.BLL.Models
{
    public class ProductsWithCategoryViewModel
    {
        public Guid ProductID { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        //public string SupplierName { get; set; }
        //public string WarehouseName { get; set; }
        //public DateTime CreatedAt { get; set; }
    }
}
