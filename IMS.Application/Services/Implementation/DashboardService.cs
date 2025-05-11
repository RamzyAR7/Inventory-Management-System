using IMS.Application.DTOs;
using IMS.Application.Services.Interface;
using IMS.Domain.Enums;
using IMS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace IMS.Application.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly IDbContextFactory<InventoryDbContext> _dbContextFactory;

        public DashboardService(IDbContextFactory<InventoryDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            using var context = _dbContextFactory.CreateDbContext();

            var totalUsers = await context.Users.CountAsync();
            var totalProducts = await context.Products.CountAsync();
            var totalOrders = await context.Orders.CountAsync();
            var totalCustomers = await context.Customers.CountAsync();
            var totalWarehouses = await context.Warehouses.CountAsync();
            var totalShipments = await context.Shipments.CountAsync();
            var totalInventory = await context.WarehouseStocks.SumAsync(ws => ws.StockQuantity);
            var totalSuppliers = await context.Suppliers.CountAsync();
            var totalCategories = await context.Categories.CountAsync();
            var totalInventoryTransactions = await context.InventoryTransactions.CountAsync();
            var totalSuppliedProducts = await context.SupplierProducts.CountAsync();
            var completedOrders = await context.Orders.CountAsync(o => o.Status == OrderStatus.Delivered);
            var pendingOrders = await context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            var canceledOrders = await context.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled);
            var processingOrders = await context.Orders.CountAsync(o => o.Status == OrderStatus.Shipped);
            var inTransitOrders = await context.Shipments.CountAsync(s => s.Status == ShipmentStatus.InTransit);

            var topProduct = await GetTopSellingProductAsync(context);
            var lowStockItems = await GetLowStockItemsCountAsync(context);
            var recentTransactions = await GetRecentTransactionsCountAsync(context);

            var totalRevenue = await context.Orders
                .Where(o => o.Status == OrderStatus.Confirmed)
                .SumAsync(o => o.TotalAmount);
            var revenueFromPendingOrders = await context.Orders
                .Where(o => o.Status == OrderStatus.Pending)
                .SumAsync(o => o.TotalAmount);
            var revenueFromCompletedOrders = await context.Orders
                .Where(o => o.Status == OrderStatus.Confirmed)
                .SumAsync(o => o.TotalAmount);

            var newCustomersThisMonth = await context.Customers
                .CountAsync(c => c.CreatedAt >= DateTime.Now.AddMonths(-1));
            var inventoryValue = await context.WarehouseStocks
                .SumAsync(ws => ws.StockQuantity * ws.Product.Price);

            var mostStockedProduct = await context.Products
                .OrderByDescending(p => p.WarehouseStocks.Sum(ws => ws.StockQuantity))
                .Select(p => p.ProductName)
                .FirstOrDefaultAsync();
            var leastStockedProduct = await context.Products
                .OrderBy(p => p.WarehouseStocks.Sum(ws => ws.StockQuantity))
                .Select(p => p.ProductName)
                .FirstOrDefaultAsync();
            var productsBelowReorderLevel = await context.WarehouseStocks
                .CountAsync(ws => ws.StockQuantity < ws.Product.RecoderLevel);

            // Calculate percentages
            var completedOrdersPercent = totalOrders > 0 ? (double)completedOrders / totalOrders * 100 : 0;
            var pendingOrdersPercent = totalOrders > 0 ? (double)pendingOrders / totalOrders * 100 : 0;
            var canceledOrdersPercent = totalOrders > 0 ? (double)canceledOrders / totalOrders * 100 : 0;
            var processingOrdersPercent = totalOrders > 0 ? (double)processingOrders / totalOrders * 100 : 0;
            var inTransitOrdersPercent = totalOrders > 0 ? (double)inTransitOrders / totalOrders * 100 : 0;

            return new DashboardDto
            {
                TotalUsers = totalUsers,
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalCustomers = totalCustomers,
                TotalWarehouses = totalWarehouses,
                TotalShipments = totalShipments,
                TotalInventory = totalInventory,
                TotalSuppliers = totalSuppliers,
                TotalCategories = totalCategories,
                TotalInventoryTransactions = totalInventoryTransactions,
                TotalSuppliedProducts = totalSuppliedProducts,
                CompletedOrders = completedOrders,
                PendingOrders = pendingOrders,
                CanceledOrders = canceledOrders,
                InTransitOrders = inTransitOrders,
                ProcessingOrders = processingOrders,
                TopSellingProductName = topProduct.Name,
                TopSellingProductQuantity = topProduct.Quantity,
                InTransactions = await context.InventoryTransactions.CountAsync(t => t.Type == TransactionType.In && t.SuppliersID != null),
                OutTransactions = await context.InventoryTransactions.CountAsync(t => t.Type == TransactionType.Out && t.OrderID != null),
                WarehouseTransfers = await context.WarehouseTransfers.CountAsync(),
                LowStockItems = lowStockItems,
                RecentTransactions = recentTransactions,
                TotalRevenue = totalRevenue,
                RevenueFromCompletedOrders = revenueFromCompletedOrders,
                RevenueFromPendingOrders = revenueFromPendingOrders,
                NewCustomersThisMonth = newCustomersThisMonth,
                MostStockedProduct = mostStockedProduct,
                LeastStockedProduct = leastStockedProduct,
                ProductsBelowReorderLevel = productsBelowReorderLevel,
                InventoryValue = inventoryValue,
                CompletedOrdersPercent = (float)Math.Round(completedOrdersPercent, 2),
                PendingOrdersPercent = (float)Math.Round(pendingOrdersPercent, 2),
                CanceledOrdersPercent = (float)Math.Round(canceledOrdersPercent, 2),
                ProcessingOrdersPercent = (float)Math.Round(processingOrdersPercent, 2),
                InTransitOrdersPercent = (float)Math.Round(inTransitOrdersPercent, 2)
            };
        }

        private async Task<(string Name, int Quantity)> GetTopSellingProductAsync(InventoryDbContext context)
        {
            var topProductGroup = await context.OrderDetails
                .GroupBy(od => od.ProductID)
                .Select(g => new { ProductID = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
                .OrderByDescending(g => g.TotalQuantity)
                .FirstOrDefaultAsync();

            if (topProductGroup == null)
                return ("N/A", 0);

            var product = await context.Products
                .Where(p => p.ProductID == topProductGroup.ProductID)
                .Select(p => p.ProductName)
                .FirstOrDefaultAsync();

            return (product ?? "N/A", topProductGroup.TotalQuantity);
        }

        private async Task<int> GetLowStockItemsCountAsync(InventoryDbContext context)
        {
            return await context.WarehouseStocks
                .Join(context.Products,
                    ws => ws.ProductID,
                    p => p.ProductID,
                    (ws, p) => new { Stock = ws, Product = p })
                .CountAsync(x => x.Stock.StockQuantity < x.Product.RecoderLevel);
        }

        private async Task<int> GetRecentTransactionsCountAsync(InventoryDbContext context)
        {
            var oneWeekAgo = DateTime.Now.AddDays(-7);
            return await context.InventoryTransactions
                .CountAsync(t => t.TransactionDate >= oneWeekAgo);
        }
    }
}
