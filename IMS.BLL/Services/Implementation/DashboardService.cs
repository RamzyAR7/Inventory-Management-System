using IMS.BLL.DTOs;
using IMS.BLL.Services.Interface;
using IMS.DAL.Context;
using IMS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMS.BLL.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly InventoryDbContext _context;
        public DashboardService(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            // Get top selling product
            var topProductGroup = await _context.OrderDetails
                .GroupBy(od => od.ProductID)
                .Select(g => new { ProductID = g.Key, TotalQuantity = g.Sum(x => x.Quantity) })
                .OrderByDescending(g => g.TotalQuantity)
                .FirstOrDefaultAsync();

            string topProductName = "N/A";
            int topProductQty = 0;
            if (topProductGroup != null)
            {
                var product = await _context.Products
                    .Where(p => p.ProductID == topProductGroup.ProductID)
                    .FirstOrDefaultAsync();
                if (product != null)
                {
                    topProductName = product.ProductName;
                    topProductQty = topProductGroup.TotalQuantity;
                }
            }

            // Get transaction counts by type
            // Get in transaction count
            int inTransactions = await _context.InventoryTransactions
              .CountAsync(t => t.Type == TransactionType.In);
            // Get out transaction count
            int outTransactions = await _context.InventoryTransactions
                .CountAsync(t => t.Type == TransactionType.Out);
            // Get warehouse transfer count
            int warehouseTransfers = await _context.WarehouseTransfers.CountAsync();

            // Get additional metrics - Modified for EF Core translation
            int lowStockItems = await _context.WarehouseStocks
                .Join(_context.Products,
                    ws => ws.ProductID,
                    p => p.ProductID,
                    (ws, p) => new { Stock = ws, Product = p })
                .CountAsync(x => x.Stock.StockQuantity < x.Product.RecoderLevel); // Fixed typo: Changed 'ReorderLevel' to 'RecoderLevel'
            int pendingOrders = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Pending);
            var oneWeekAgo = DateTime.Now.AddDays(-7);
            int recentTransactions = await _context.InventoryTransactions
                .CountAsync(t => t.TransactionDate >= oneWeekAgo);

            var dto = new DashboardDto
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalCustomers = await _context.Customers.CountAsync(),
                TotalWarehouses = await _context.Warehouses.CountAsync(),
                TotalShipments = await _context.Shipments.CountAsync(),
                TotalInventory = await _context.WarehouseStocks.SumAsync(ws => ws.StockQuantity),
                TotalSuppliers = await _context.Suppliers.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalInventoryTransactions = await _context.InventoryTransactions.CountAsync(),
                TotalSuppliedProducts = await _context.SupplierProducts.CountAsync(),
                CompletedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Confirmed),
                TopSellingProductName = topProductName,
                TopSellingProductQuantity = topProductQty,
                // New transaction summary data
                InTransactions = inTransactions,
                OutTransactions = outTransactions,
                WarehouseTransfers = warehouseTransfers,
                // Additional metrics
                LowStockItems = lowStockItems,
                PendingOrders = pendingOrders,
                RecentTransactions = recentTransactions
            };

            return dto;
        }

    }
}
