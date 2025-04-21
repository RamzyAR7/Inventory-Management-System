using Inventory_Management_System.Entities;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.Category
{
    public class CategoryResDto
    {
        public Guid CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public ICollection<ProdictsCategoryDto> Products { get; set; }
    }
}
