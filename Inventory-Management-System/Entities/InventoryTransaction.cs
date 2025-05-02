namespace Inventory_Management_System.Entities
{
    public class InventoryTransaction
    {
        public Guid TransactionID { get; set; }
        public TransactionType Type { get; set; }
        public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public Guid WarehouseID { get; set; }
        public Warehouse Warehouse { get; set; }
        public Guid ProductID { get; set; }
        public Product Product { get; set; }
        public Guid? SuppliersID { get; set; }
        public Supplier Suppliers { get; set; }
        public Guid? OrderID { get; set; }
        public Order Order { get; set; }

        public ICollection<WarehouseTransfers> InTransfers { get; set; }
        public ICollection<WarehouseTransfers> OutTransfers { get; set; }
    }
}
