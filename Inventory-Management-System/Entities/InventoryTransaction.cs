namespace Inventory_Management_System.Entities
{
    public class InventoryTransaction
    {
        public Guid TransactionID { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string Reference { get; set; } = string.Empty;

        public Guid WarehouseID { get; set; }
        public Warehouse Warehouse { get; set; } = new();

        public Guid ProductID { get; set; }
        public Product Product { get; set; } = new();
    }
}
