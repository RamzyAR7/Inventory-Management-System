using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Inventory_Management_System.Entities;

namespace Inventory_Management_System.Models.DTOs.Products
{
    public class ProductReqDto
    {
        [Key]
        public Guid ProductID { get; set; }
        [Required(ErrorMessage = "Product Name not found"), MaxLength(100)]
        public string ProductName { get; set; }
        [Required(ErrorMessage = "Product Description not found")]
        public string ProductDescription { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        [Required(ErrorMessage = "Price not found")]
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Recoder Level not found")]
        public int RecoderLevel { get; set; }
        [Required(ErrorMessage = "Category not found")]
        public Guid CategoryID { get; set; }
        [Required(ErrorMessage = "Supplier not found")]
        public List<Guid> SuppliersIDs { get; set; } = new();

    }
}
