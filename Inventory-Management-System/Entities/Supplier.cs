using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Entities
{
    public class Supplier
    {
        public Guid SupplierID { get; set; }
        [Required, MaxLength(100)]
        public string SupplierName { get; set; }
        [Required, MaxLength(15)]
        public string PhoneNumber { get; set; }
        [MaxLength(255)]
        public string Email { get; set; }

        // navigation properties

        public ICollection<SupplierProduct> SupplierProducts { get; set; }
    }
}
