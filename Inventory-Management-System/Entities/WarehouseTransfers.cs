namespace Inventory_Management_System.Entities
{
    public class WarehouseTransfers
    {
        public Guid WarehouseTransferID { get; set; }

        public Guid ProductID { get; set; }
        public Product Product { get; set; }

        public Guid FromWarehouseID { get; set; }
        public Warehouse FromWarehouse { get; set; }

        public Guid ToWarehouseID { get; set; }
        public Warehouse ToWarehouse { get; set; }

        public Guid OutTransactionID { get; set; }
        public InventoryTransaction OutTransaction { get; set; }

        public Guid InTransactionID { get; set; }
        public InventoryTransaction InTransaction { get; set; }

        public int Quantity { get; set; } // quantity being transferred
        public DateTime TransferDate { get; set; }
    }
}
