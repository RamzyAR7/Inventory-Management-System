using IMS.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS.BLL.DTOs.Products
{
    public class ProductReqDto
    {
        [Required(ErrorMessage = "Product Name is required"), MaxLength(100)]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Product Description is required")]
        public string ProductDescription { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Required(ErrorMessage = "Price is required")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Reorder Level is required")]
        public int RecoderLevel { get; set; }
        [Required]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Category is required")]
        public Guid CategoryID { get; set; }

        [Required(ErrorMessage = "At least one warehouse is required")]
        public List<Guid> WarehouseIds { get; set; } = new List<Guid>(); // Changed to List<Guid>

        public ICollection<WarehouseStock> WarehouseStocks { get; set; } = new List<WarehouseStock>();
    }
}
