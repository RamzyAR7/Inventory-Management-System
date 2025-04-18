using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.DataAccess.Context;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
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
                TopSellingProductQuantity = topProductQty
            };

            return dto;
        }
    }
}
