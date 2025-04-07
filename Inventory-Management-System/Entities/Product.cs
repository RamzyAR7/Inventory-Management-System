using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory_Management_System.Entities
{
    public class Product
    {
        public Guid ProductID { get; set; }
        [Required, MaxLength(100)]
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public int RecoderLevel { get; set; }

        // foreign key to Category
        public Guid CategoryID { get; set; }

        // navigation properties
        public Category Category { get; set; }
        public ICollection<SupplierProduct> SupplierProducts { get; set; }
        public ICollection<WarehouseStock> WarehouseStocks { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; }
    }
}
