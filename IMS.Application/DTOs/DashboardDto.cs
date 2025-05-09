namespace IMS.Application.DTOs
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

        public int InTransactions { get; set; }
        public int OutTransactions { get; set; }
        public int WarehouseTransfers { get; set; }

        public int LowStockItems { get; set; }
        public int PendingOrders { get; set; }
        public int RecentTransactions { get; set; } // Transactions in the last 7 days
    }

}
