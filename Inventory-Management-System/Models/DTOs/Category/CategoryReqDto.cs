using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.Category
{
    public class CategoryReqDto
    {
        [Required]
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }
}
