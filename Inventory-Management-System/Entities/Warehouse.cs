namespace Inventory_Management_System.Entities
{

    public class Warehouse
    {
        public Guid WarehouseID { get; set; }
        public string Address { get; set; }
        public string WarehouseName { get; set; }
        public Guid ManagerID { get; set; }
        public User Manager { get; set; }

        public ICollection<WarehouseStock> WarehouseStocks { get; set; }
        public ICollection<Shipment> Shipments { get; set; }

        public ICollection<InventoryTransaction> InventoryTransactions { get; set; }
    }
}
