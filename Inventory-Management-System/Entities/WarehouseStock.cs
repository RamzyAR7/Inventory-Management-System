namespace Inventory_Management_System.Entities
{
    public class WarehouseStock
    {
        // foreign key to Warehouse
        public Guid WarehouseID { get; set; }
        // foreign key to Product
        public Guid ProductID { get; set; }

        public int StockQuantity { get; set; } = 0;

        // navigation properties
        public Warehouse Warehouse { get; set; }
        public Product Product { get; set; }

    }
}
