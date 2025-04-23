namespace Inventory_Management_System.Entities
{
    public class InventoryTransaction
    {
        public Guid TransactionID { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Reference { get; set; }

        public Guid WarehouseID { get; set; }
        public Warehouse Warehouse { get; set; }
        public Guid ProductID { get; set; }
        public Product Product { get; set; }
    }
}