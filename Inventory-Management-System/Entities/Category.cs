using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Entities
{
    public class Category
    {
        public Guid CategoryID { get; set; }
        [Required, MaxLength(100)]
        public string CategoryName { get; set; }
        public string Description { get; set; }

        // navigation properties
        public ICollection<Product> Products { get; set; }
    }
}
