namespace Inventory_Management_System.Entities
{
    public class SupplierProduct
    {
        // foreign key to Supplier
        public Guid SupplierID { get; set; }
        // foreign key to Product
        public Guid ProductID { get; set; }

        // navigation properties
        public Supplier Supplier { get; set; }
        public Product Product { get; set; }
    }
}
