using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.Category
{
    public class ProdictsCategoryDto
    {
        public Guid ProductID { get; set; }
        public string ProductName { get; set; }
        public Guid CategoryID { get; set; }
        public CategoryResDto Category { get; set; }

    }
}
