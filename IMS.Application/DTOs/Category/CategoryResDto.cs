using System.ComponentModel.DataAnnotations;
using IMS.Domain.Entities;

namespace IMS.Application.DTOs.Category
{
    public class CategoryResDto
    {
        public Guid CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
