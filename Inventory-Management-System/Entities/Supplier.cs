using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Entities
{
    public class Supplier
    {
        #region Properties
        public Guid SupplierID { get; set; }
        [Required, MaxLength(100)]
        public string SupplierName { get; set; }
        [Required, MaxLength(15)]
        public string PhoneNumber { get; set; }
        [MaxLength(255)]
        public string Email { get; set; }

        // Navigation properties
        public ICollection<SupplierProduct> SupplierProducts { get; set; }
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; }
        #endregion
    }
}
