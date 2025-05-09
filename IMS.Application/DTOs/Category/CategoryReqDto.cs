using System.ComponentModel.DataAnnotations;

namespace IMS.Application.DTOs.Category
{
    public class CategoryReqDto
    {
        [Required]
        public string CategoryName { get; set; }
        public string Description { get; set; }
    }
}
