namespace Inventory_Management_System.Entities
{

    public class Warehouse
    {
        public Guid WarehouseID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Capacity { get; set; }

        public string Address { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public Guid ManagerID { get; set; }
        public User Manager { get; set; } = new();


        public ICollection<WarehouseStock> WarehouseStocks { get; set; } = new List<WarehouseStock>();
        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    }
}