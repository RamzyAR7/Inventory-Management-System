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
        private readonly IUnitOfWork _unitOfWork;

        public TransactionsController(ITransactionService transactionService, IWarehouseService warehouseService, IProductService productService, IUnitOfWork unitOfWork)
        {
            _transactionService = transactionService;
            _warehouseService = warehouseService;
            _productService = productService;
            _unitOfWork = unitOfWork;
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
                t => t.Product
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
                t => t.Product
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
        public async Task<IActionResult> CreateInOrOutTransaction(Guid? warehouseId = null)
        {
            await PopulateViewBagAsync(warehouseId);
            return View(new CreateInventoryTransactionDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateInOrOutTransaction(CreateInventoryTransactionDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _transactionService.CreateInOrOutTransactionAsync(dto);
                    TempData["SuccessMessage"] = "Transaction completed successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error: {ex.Message}";
                }
            }
            await PopulateViewBagAsync(dto.WarehouseId, dto.ProductId);
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> TransferBetweenWarehouses(Guid? fromWarehouseId = null, Guid? toWarehouseId = null, Guid? productId = null, Guid? toProductId = null)
        {
            await PopulateViewBagAsync(fromWarehouseId, productId, toWarehouseId, toProductId);
            return View(new CreateWarehouseTransferDto());
        }

        [HttpPost]
        public async Task<IActionResult> TransferBetweenWarehouses(CreateWarehouseTransferDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _transactionService.TransferBetweenWarehousesAsync(dto);
                    TempData["SuccessMessage"] = "Warehouse transfer completed successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }
            await PopulateViewBagAsync(dto.FromWarehouseId, dto.ProductId, dto.ToWarehouseId, dto.ToProductId);
            return View(dto);
        }

        private async Task PopulateViewBagAsync(Guid? selectedWarehouseId = null, Guid? selectedProductId = null, Guid? selectedToWarehouseId = null, Guid? selectedToProductId = null)
        {
            var warehouseDtos = await _warehouseService.GetAllAsync();
            var products = await _productService.GetAllAsync();
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
                var warehouseStocks = await _unitOfWork.WarehouseStocks
                    .Find(ws => ws.WarehouseID == selectedWarehouseId.Value)
                    .Include(ws => ws.Product)
                    .ToListAsync();
                var availableProductIdsWithStock = warehouseStocks
                    .Where(ws => ws.StockQuantity > 0)
                    .Select(ws => new { ws.ProductID, ws.StockQuantity })
                    .ToList();
                var availableProducts = products
                    .Where(p => availableProductIdsWithStock.Select(x => x.ProductID).Contains(p.ProductID))
                    .Select(p => new
                    {
                        p.ProductID,
                        DisplayText = $"{p.ProductName} (Stock: {availableProductIdsWithStock.FirstOrDefault(x => x.ProductID == p.ProductID)?.StockQuantity})"
                    })
                    .ToList();
                ViewBag.Products = new SelectList(
                    availableProducts,
                    "ProductID",
                    "DisplayText",
                    selectedProductId);

                if (selectedProductId.HasValue)
                {
                    var selectedStock = warehouseStocks.FirstOrDefault(ws => ws.ProductID == selectedProductId.Value);
                    if (selectedStock == null || selectedStock.StockQuantity <= 0)
                    {
                        ModelState.AddModelError("ProductId", $"The selected product is out of stock in the source warehouse. Available stock: {selectedStock?.StockQuantity ?? 0}");
                    }
                }
            }
            else
            {
                ViewBag.Products = new SelectList(
                    products.Select(p => new { p.ProductID, DisplayText = $"{p.ProductName} (Stock: N/A)" }),
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
                    var matchingProductsWithStock = toWarehouseStocks
                        .Where(ws => ws.Product != null && ws.Product.ProductName.ToLower() == fromProduct.ProductName.ToLower())
                        .Select(ws => new
                        {
                            ws.Product.ProductID,
                            DisplayText = $"{ws.Product.ProductName} (Stock: {ws.StockQuantity})"
                        })
                        .Distinct()
                        .ToList();
                    ViewBag.ToProducts = new SelectList(
                        matchingProductsWithStock,
                        "ProductID",
                        "DisplayText",
                        selectedToProductId);

                    if (selectedToProductId.HasValue)
                    {
                        var selectedToProduct = matchingProductsWithStock.FirstOrDefault(p => p.ProductID == selectedToProductId.Value);
                        if (selectedToProduct == null)
                        {
                            ModelState.AddModelError("ToProductId", "The selected destination product does not match the source product.");
                        }
                    }
                }
                else
                {
                    ViewBag.ToProducts = new SelectList(Enumerable.Empty<object>(), "ProductID", "DisplayText", selectedToProductId);
                }
            }
            else
            {
                ViewBag.ToProducts = new SelectList(Enumerable.Empty<object>(), "ProductID", "DisplayText", selectedToProductId);
            }
        }
    }
}
