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
        public int CanceledOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public float CompletedOrdersPercent { get; set; }
        public float PendingOrdersPercent { get; set; }
        public int InTransitOrders { get; set; }
        public float InTransitOrdersPercent { get; set; }
        public float CanceledOrdersPercent { get; set; }
        public float ProcessingOrdersPercent { get; set; }
        public int LowStockItems { get; set; }
        public int PendingOrders { get; set; }
        public int RecentTransactions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueFromCompletedOrders { get; set; }
        public decimal RevenueFromPendingOrders { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public string MostStockedProduct { get; set; }
        public string LeastStockedProduct { get; set; }
        public int ProductsBelowReorderLevel { get; set; }
        public decimal InventoryValue { get; set; }
    }
}