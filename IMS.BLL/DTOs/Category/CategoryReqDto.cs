using System.ComponentModel.DataAnnotations;

namespace IMS.BLL.DTOs.Category
{
    public class CategoryReqDto
    {
        [Required]
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }
}
