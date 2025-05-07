using System.ComponentModel.DataAnnotations;

namespace IMS.Domain.Entities
{
    public class Category
    {
        #region Properties
        public Guid CategoryID { get; set; }
        [Required, MaxLength(100)]
        public string CategoryName { get; set; }
        public string Description { get; set; }
        // Navigation properties
        public ICollection<Product> Products { get; set; }
        #endregion
    }
}