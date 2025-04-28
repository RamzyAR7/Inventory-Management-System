using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.InventoryTransaction;
using Inventory_Management_System.Models.DTOs.Warehouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Inventory_Management_System.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IWarehouseService _warehouseService;
        private readonly IProductService _productService;
        private readonly ISupplierService _supplierService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionsController> _logger;


        public TransactionsController(ITransactionService transactionService, IWarehouseService warehouseService, IProductService productService, ISupplierService supplierService, IUnitOfWork unitOfWork, ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _warehouseService = warehouseService;
            _productService = productService;
            _supplierService = supplierService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid? warehouseId = null, int pageNumber = 1, int pageSize = 10)
        {
            var includes = new Expression<Func<InventoryTransaction, object>>[]
            {
                t => t.Warehouse,
                t => t.Product
            };
            var (transactions, totalCount) = warehouseId.HasValue
                ? await _unitOfWork.InventoryTransactions.GetPagedAsync(pageNumber, pageSize, t => t.WarehouseID == warehouseId.Value, includes)
                : await _unitOfWork.InventoryTransactions.GetPagedAsync(pageNumber, pageSize, null, includes);

            var transferIncludes = new Expression<Func<WarehouseTransfers, object>>[]
            {
                t => t.FromWarehouse,
                t => t.ToWarehouse,
                t => t.ToProduct,
                t => t.FromProduct
            };
            var transfers = await _unitOfWork.WarehouseTransfers.GetAllAsync(transferIncludes);

            var warehouses = await _warehouseService.GetAllAsync();
            ViewBag.Warehouses = new SelectList(warehouses, "WarehouseID", "WarehouseName", warehouseId);
            ViewBag.Transfers = transfers;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;

            return View(transactions);
        }

        [HttpGet]
        public async Task<IActionResult> ListTransactions(Guid? warehouseId = null, int pageNumber = 1, int pageSize = 10)
        {
            var includes = new Expression<Func<InventoryTransaction, object>>[]
            {
                t => t.Warehouse,
                t => t.Product
            };
            var (transactions, totalCount) = warehouseId.HasValue
                ? await _unitOfWork.InventoryTransactions.GetPagedAsync(pageNumber, pageSize, t => t.WarehouseID == warehouseId.Value, includes)
                : await _unitOfWork.InventoryTransactions.GetPagedAsync(pageNumber, pageSize, null, includes);

            ViewBag.TotalCount = totalCount;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            await PopulateViewBagAsync(warehouseId);
            return View(transactions);
        }

        [HttpGet]
        public async Task<IActionResult> ListTransfers(int pageNumber = 1, int pageSize = 10)
        {
            var includes = new Expression<Func<WarehouseTransfers, object>>[]
            {
                t => t.FromWarehouse,
                t => t.ToWarehouse,
                t => t.ToProduct,
                t => t.FromProduct
            };
            var (transfers, totalCount) = await _unitOfWork.WarehouseTransfers.GetPagedAsync(pageNumber, pageSize, null, includes);

            ViewBag.TotalCount = totalCount;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            await PopulateViewBagAsync();
            return View(transfers);
        }

        [HttpGet]
        public async Task<IActionResult> TransactionDetails(Guid id)
        {
            try
            {
                var transaction = await _transactionService.GetTransactionByIdAsync(id);
                return View(transaction);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TransferDetails(Guid id)
        {
            try
            {
                var transfer = await _transactionService.GetTransferByIdAsync(id);
                return View(transfer);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateInTransaction(Guid? warehouseId = null)
        {
            await PopulateViewBagAsync(warehouseId);
            return View(new CreateInventoryTransactionDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateInTransaction(CreateInventoryTransactionDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _transactionService.CreateInTransactionAsync(dto);
                    TempData["SuccessMessage"] = "Transaction completed successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error: {ex.Message}";
                }
            }
            await PopulateViewBagAsync(dto.SupplierID ,dto.WarehouseId, dto.ProductId);
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> TransferBetweenWarehouses()
        {
            await PopulateViewBagAsync(requireStockForSource: true);
            return View(new CreateWarehouseTransferDto());
        }

        [HttpPost]
        public async Task<IActionResult> TransferBetweenWarehouses(CreateWarehouseTransferDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var fromProduct = await _unitOfWork.Products.GetByIdAsync(dto.FromProductId);
                    var toProduct = await _unitOfWork.Products.GetByIdAsync(dto.ToProductId);
                    if (fromProduct == null || toProduct == null)
                    {
                        throw new Exception("Source or destination product not found.");
                    }

                    if (fromProduct.ProductName.ToLower() != toProduct.ProductName.ToLower())
                    {
                        throw new Exception("Source and destination products must have the same name.");
                    }

                    await _transactionService.TransferBetweenWarehousesAsync(dto);
                    await _productService.AssignSupplierFromAnotherProductAsync(dto.FromProductId, dto.ToProductId);
                    TempData["SuccessMessage"] = "Transfer completed successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    await PopulateViewBagAsync(null, dto.FromWarehouseId, dto.FromProductId, dto.ToWarehouseId, dto.ToProductId, requireStockForSource: true);
                }
            }
            else
            {
                await PopulateViewBagAsync(null, dto.FromWarehouseId, dto.FromProductId, dto.ToWarehouseId, dto.ToProductId, requireStockForSource: true);
            }

            return View(dto);
        }
        private async Task PopulateViewBagAsync(Guid? selectedSupplierId = null, Guid? selectedWarehouseId = null, Guid? selectedProductId = null, Guid? selectedToWarehouseId = null, Guid? selectedToProductId = null, bool requireStockForSource = false)
        {
            var warehouseDtos = await _warehouseService.GetAllAsync();
            var products = await _productService.GetAllAsync();
            var suppliers = await _supplierService.GetAllAsync();
            IEnumerable<WarehouseResDto> filteredWarehouseDtos = warehouseDtos;

            if (User.IsInRole("Manager"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId != null)
                {
                    filteredWarehouseDtos = warehouseDtos.Where(w => w.ManagerID == Guid.Parse(userId)).ToList();
                    if (!filteredWarehouseDtos.Any())
                    {
                        ModelState.AddModelError(string.Empty, "No warehouse assigned to this manager.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                    filteredWarehouseDtos = new List<WarehouseResDto>();
                }
            }

            ViewBag.Suppliers = new SelectList(
                suppliers.Select(s => new { s.SupplierID, s.SupplierName }),
                "SupplierID",
                "SupplierName",
                selectedSupplierId);

            ViewBag.Warehouses = new SelectList(
                filteredWarehouseDtos.Select(w => new { w.WarehouseID, w.WarehouseName }),
                "WarehouseID",
                "WarehouseName",
                selectedWarehouseId);

            ViewBag.ToWarehouses = new SelectList(
                filteredWarehouseDtos.Select(w => new { w.WarehouseID, w.WarehouseName }),
                "WarehouseID",
                "WarehouseName",
                selectedToWarehouseId);

            // Populate Products for source warehouse
            if (selectedWarehouseId.HasValue)
            {
                var warehouseStocksQuery = _unitOfWork.WarehouseStocks.Find(ws => ws.WarehouseID == selectedWarehouseId.Value);
                if (requireStockForSource)
                {
                    warehouseStocksQuery = warehouseStocksQuery.Where(ws => ws.StockQuantity > 0);
                }
                var warehouseStocks = await warehouseStocksQuery.Include(ws => ws.Product).ToListAsync();

                var availableProducts = warehouseStocks
                    .Select(ws => new
                    {
                        ws.Product.ProductID,
                        DisplayText = $"{ws.Product.ProductName} (Stock: {ws.StockQuantity})"
                    })
                    .DistinctBy(p => p.ProductID)
                    .OrderBy(p => p.DisplayText)
                    .ToList();

                if (!availableProducts.Any() && warehouseStocks.Any())
                {
                    _logger.LogWarning("Mismatch between WarehouseStock ProductIDs and Products table for WarehouseID {WarehouseID}", selectedWarehouseId.Value);
                    ModelState.AddModelError("FromProductId", "No products found for the selected warehouse due to a data mismatch.");
                }
                else if (!warehouseStocks.Any())
                {
                    ModelState.AddModelError("FromProductId", "The selected warehouse has no products assigned.");
                }

                ViewBag.Products = new SelectList(
                    availableProducts,
                    "ProductID",
                    "DisplayText",
                    selectedProductId);

                if (requireStockForSource && selectedProductId.HasValue)
                {
                    var selectedStock = warehouseStocks.FirstOrDefault(ws => ws.ProductID == selectedProductId.Value);
                    if (selectedStock == null || selectedStock.StockQuantity <= 0)
                    {
                        ModelState.AddModelError("FromProductId", $"The selected product is out of stock in the source warehouse. Available stock: {selectedStock?.StockQuantity ?? 0}");
                    }
                }
            }
            else
            {
                ViewBag.Products = new SelectList(
                    new List<object>(),
                    "ProductID",
                    "DisplayText",
                    selectedProductId);
            }

            // Populate ToProducts for destination warehouse
            if (selectedToWarehouseId.HasValue && selectedProductId.HasValue)
            {
                var fromProduct = products.FirstOrDefault(p => p.ProductID == selectedProductId.Value);
                if (fromProduct != null)
                {
                    var toWarehouseStocks = await _unitOfWork.WarehouseStocks
                        .Find(ws => ws.WarehouseID == selectedToWarehouseId.Value)
                        .Include(ws => ws.Product)
                        .ToListAsync();

                    var matchingProducts = toWarehouseStocks
                        .Where(ws => ws.Product.ProductName.ToLower() == fromProduct.ProductName.ToLower())
                        .Select(ws => new
                        {
                            ProductID = ws.Product.ProductID,
                            DisplayText = $"{ws.Product.ProductName} (Stock: {ws.StockQuantity})"
                        })
                        .DistinctBy(p => p.ProductID)
                        .OrderBy(p => p.DisplayText)
                        .ToList();

                    ViewBag.ToProducts = new SelectList(
                        matchingProducts,
                        "ProductID",
                        "DisplayText",
                        selectedToProductId);

                    if (selectedToProductId.HasValue && !matchingProducts.Any(p => p.ProductID == selectedToProductId.Value))
                    {
                        ModelState.AddModelError("ToProductId", "The selected destination product does not match the source product.");
                    }
                }
                else
                {
                    ViewBag.ToProducts = new SelectList(new List<object>(), "ProductID", "DisplayText", selectedToProductId);
                    ModelState.AddModelError("ToProductId", "Source product not found.");
                }
            }
            else
            {
                ViewBag.ToProducts = new SelectList(new List<object>(), "ProductID", "DisplayText", selectedToProductId);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetProductsByWarehouse(Guid warehouseId)
        {
            try
            {
                var warehouseStocks = await _unitOfWork.WarehouseStocks
                    .Find(ws => ws.WarehouseID == warehouseId)
                    .Include(ws => ws.Product)
                    .ToListAsync();

                var products = warehouseStocks
                    .GroupBy(ws => ws.Product.ProductName.ToLower())
                    .Select(g => g.First())
                    .Select(ws => new
                    {
                        ProductID = ws.Product.ProductID,
                        DisplayText = $"{ws.Product.ProductName} (Stock: {ws.StockQuantity})"
                    })
                    .OrderBy(p => p.DisplayText)
                    .ToList();

                _logger.LogInformation("Fetched {Count} products for WarehouseID {WarehouseID}", products.Count, warehouseId);
                return Json(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products for WarehouseID {WarehouseID}", warehouseId);
                return StatusCode(500, new { error = "Error fetching products" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMatchingProducts(Guid productId, Guid toWarehouseId)
        {
            try
            {
                var products = await _productService.GetAllAsync();
                var fromProduct = products.FirstOrDefault(p => p.ProductID == productId);
                if (fromProduct == null)
                {
                    _logger.LogWarning("Source product not found for ProductID {ProductID}", productId);
                    return Json(new List<object>());
                }

                // Get all products in the destination warehouse
                var toWarehouseStocks = await _unitOfWork.WarehouseStocks
                    .Find(ws => ws.WarehouseID == toWarehouseId)
                    .Include(ws => ws.Product)
                    .ToListAsync();

                // Filter products by name and ensure they are in the destination warehouse
                var matchingProducts = toWarehouseStocks
                    .Where(ws => ws.Product.ProductName.ToLower() == fromProduct.ProductName.ToLower())
                    .Select(ws => new
                    {
                        ProductID = ws.Product.ProductID,
                        DisplayText = $"{ws.Product.ProductName} (Stock: {ws.StockQuantity})"
                    })
                    .DistinctBy(p => p.ProductID) // Ensure no duplicates by ProductID
                    .OrderBy(p => p.DisplayText)
                    .ToList();

                _logger.LogInformation("Found {Count} matching products for ProductID {ProductID} in WarehouseID {WarehouseID}", matchingProducts.Count, productId, toWarehouseId);
                return Json(matchingProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching matching products for ProductID {ProductID}, ToWarehouseID {ToWarehouseID}", productId, toWarehouseId);
                return StatusCode(500, new { error = "Error fetching matching products" });
            }
        }

    }
}
