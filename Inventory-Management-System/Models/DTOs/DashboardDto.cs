namespace Inventory_Management_System.Models.DTOs
{
    public class DashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalWarehouses { get; set; }
        public int TotalShipments { get; set; }
        public int TotalInventory { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalCategories { get; set; }
        public int TotalInventoryTransactions { get; set; }
        public int TotalSuppliedProducts { get; set; }
        public int CompletedOrders { get; set; }

        public string TopSellingProductName { get; set; }
        public int TopSellingProductQuantity { get; set; }
    }

}
