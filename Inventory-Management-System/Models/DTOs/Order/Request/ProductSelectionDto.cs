using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.Order.Request
{
    public class ProductSelectionDto
    {
        public Guid ProductID { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public Guid CategoryID { get; set; }

        [Required]
        public string CategoryName { get; set; }

        public Guid WarehouseID { get; set; }

        [Required]
        public string WarehouseName { get; set; }
    }
}
