
using Inventory_Management_System.ViewModels;
using Inventory_Management_System.DataAccess.Context;
using Microsoft.AspNetCore.Mvc;

public class InventoryController : Controller
{
    private readonly InventoryDbContext _context;

    public InventoryController(InventoryDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var model = _context.Products.Select(product => new ProductInventoryViewModel
        {
            ProductName = product.ProductName,
            Category = product.Category.CategoryName,
            Price = product.Price,
            Stock = _context.InventoryTransactions
                      .Where(t => t.ProductID == product.ProductID && t.Type == "In")
                      .Sum(t => t.Quantity)
                    - _context.InventoryTransactions
                      .Where(t => t.ProductID == product.ProductID && t.Type == "Out")
                      .Sum(t => t.Quantity),
            LastUpdated = _context.InventoryTransactions
                            .Where(t => t.ProductID == product.ProductID)
                            .OrderByDescending(t => t.TransactionDate)
                            .Select(t => t.TransactionDate)
                            .FirstOrDefault()
        }).ToList();

        return View(model);
    }
}
