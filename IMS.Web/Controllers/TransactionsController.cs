using IMS.BLL.DTOs.Transactions;
using IMS.BLL.DTOs.Warehouse;
using IMS.BLL.Interfaces;
using IMS.BLL.Services.Interface;
using IMS.BLL.SharedServices.Interface;
using IMS.DAL.UnitOfWork;
using IMS.Domain.Entities;
using IMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IMS.Web.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IWarehouseService _warehouseService;
        private readonly IProductService _productService;
        private readonly ISupplierService _supplierService;
        private readonly IProductHelperService _productHelperService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ITransactionService transactionService, IWarehouseService warehouseService, IProductHelperService productHelperService, IProductService productService, ISupplierService supplierService, IUnitOfWork unitOfWork, ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _warehouseService = warehouseService;
            _productService = productService;
            _productHelperService = productHelperService;
            _supplierService = supplierService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid? warehouseId = null)
        {
            try
            {
                var (supplierInTransactions, customerOutTransactions, transfers) = await _transactionService.GetLimitedTransactionsAndTransfersAsync(warehouseId);

                var warehouses = await _warehouseService.GetAllAsync();
                if (User.IsInRole("Manager"))
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userId != null)
                    {
                        warehouses = warehouses.Where(w => w.ManagerID == Guid.Parse(userId)).ToList();
                        _logger.LogInformation("Index - Filtered warehouses for Manager UserID {UserID}: {WarehouseCount}", userId, warehouses.Count());
                    }
                    else
                    {
                        _logger.LogWarning("Index - UserID not found in claims for Manager role");
                    }
                }

                ViewBag.Warehouses = new SelectList(warehouses, "WarehouseID", "WarehouseName", warehouseId);
                ViewBag.SupplierInTransactions = supplierInTransactions;
                ViewBag.CustomerOutTransactions = customerOutTransactions;
                ViewBag.Transfers = transfers;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Index - Error retrieving transactions: {Message}", ex.Message);
                TempData["ErrorMessage"] = "An error occurred while retrieving transactions.";
                ViewBag.SupplierInTransactions = new List<InventoryTransaction>();
                ViewBag.CustomerOutTransactions = new List<InventoryTransaction>();
                ViewBag.Transfers = new List<WarehouseTransfers>();
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListTransactions(Guid? warehouseId = null, int pageNumber = 1, int pageSize = 10, string searchSupplier = null, string searchCustomer = null, string sortColumn = "TransactionDate", bool sortAscending = false)
        {
            try
            {
                _logger.LogInformation("ListTransactions - Retrieving transactions for WarehouseID: {WarehouseID}, PageNumber: {PageNumber}, PageSize: {PageSize}, SearchSupplier: {SearchSupplier}, SearchCustomer: {SearchCustomer}, SortColumn: {SortColumn}, SortAscending: {SortAscending}", warehouseId, pageNumber, pageSize, searchSupplier, searchCustomer, sortColumn, sortAscending);

                var (transactions, totalCount) = await _transactionService.GetPagedTransactionsAsync(warehouseId, pageNumber, pageSize, searchSupplier, searchCustomer, sortColumn, sortAscending);
                _logger.LogInformation("ListTransactions - Retrieved {TransactionCount} transactions, TotalCount: {TotalCount}", transactions.Count(), totalCount);

                var supplierInTransactions = transactions
                    .Where(t => t.Type == TransactionType.In && t.SuppliersID != null)
                    .ToList();
                var customerOutTransactions = transactions
                    .Where(t => t.Type == TransactionType.Out && t.OrderID != null)
                    .ToList();

                ViewBag.SupplierInTransactions = supplierInTransactions;
                ViewBag.CustomerOutTransactions = customerOutTransactions;
                ViewBag.TotalCount = totalCount;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.WarehouseId = warehouseId;
                ViewBag.SearchSupplier = searchSupplier;
                ViewBag.SearchCustomer = searchCustomer;
                ViewBag.SortColumn = sortColumn;
                ViewBag.SortAscending = sortAscending;
                await PopulateViewBagAsync(warehouseId);

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ListTransactions - Error retrieving transactions: {Message}", ex.Message);
                TempData["error"] = "An error occurred while retrieving transactions.";
                ViewBag.SupplierInTransactions = new List<InventoryTransaction>();
                ViewBag.CustomerOutTransactions = new List<InventoryTransaction>();
                ViewBag.TotalCount = 0;
                ViewBag.PageNumber = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.WarehouseId = warehouseId;
                ViewBag.SearchSupplier = searchSupplier;
                ViewBag.SearchCustomer = searchCustomer;
                ViewBag.SortColumn = sortColumn;
                ViewBag.SortAscending = sortAscending;
                await PopulateViewBagAsync(warehouseId);
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ListTransfers(Guid? warehouseId = null, int pageNumber = 1, int pageSize = 10, string sortColumn = "TransferDate", bool sortAscending = false)
        {
            try
            {
                _logger.LogInformation("ListTransfers - Retrieving transfers, WarehouseID: {WarehouseID}, PageNumber: {PageNumber}, PageSize: {PageSize}, SortColumn: {SortColumn}, SortAscending: {SortAscending}", warehouseId, pageNumber, pageSize, sortColumn, sortAscending);

                var (transfers, totalCount) = await _transactionService.GetPagedTransfersAsync(warehouseId, pageNumber, pageSize, sortColumn, sortAscending);
                _logger.LogInformation("ListTransfers - Retrieved {TransferCount} transfers, TotalCount: {TotalCount}", transfers.Count(), totalCount);

                ViewBag.TotalCount = totalCount;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.WarehouseId = warehouseId;
                ViewBag.SortColumn = sortColumn;
                ViewBag.SortAscending = sortAscending;
                await PopulateViewBagAsync(warehouseId);

                return View(transfers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ListTransfers - Error retrieving transfers: {Message}", ex.Message);
                TempData["error"] = "An error occurred while retrieving transfers.";
                ViewBag.TotalCount = 0;
                ViewBag.PageNumber = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.WarehouseId = warehouseId;
                ViewBag.SortColumn = sortColumn;
                ViewBag.SortAscending = sortAscending;
                await PopulateViewBagAsync(warehouseId);
                return View(new List<WarehouseTransfers>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> TransactionDetails(Guid id)
        {
            try
            {
                _logger.LogInformation("TransactionDetails - Retrieving details for TransactionID: {TransactionID}", id);
                var transaction = await _transactionService.GetTransactionByIdAsync(id);
                return View(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TransactionDetails - Error retrieving transaction details for TransactionID {TransactionID}: {Message}", id, ex.Message);
                TempData["error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> TransferDetails(Guid id)
        {
            try
            {
                _logger.LogInformation("TransferDetails - Retrieving details for TransferID: {TransferID}", id);
                var transfer = await _transactionService.GetTransferByIdAsync(id);
                return View(transfer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TransferDetails - Error retrieving transfer details for TransferID {TransferID}: {Message}", id, ex.Message);
                TempData["error"] = $"Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateInTransaction(Guid? warehouseId = null)
        {
            try
            {
                _logger.LogInformation("CreateInTransaction GET - Loading form for WarehouseID: {WarehouseID}", warehouseId);
                await PopulateViewBagAsync(warehouseId);
                return View(new CreateInventoryTransactionDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateInTransaction GET - Error loading form: {Message}", ex.Message);
                TempData["error"] = "An error occurred while loading the transaction form.";
                return View(new CreateInventoryTransactionDto());
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateInTransaction(CreateInventoryTransactionDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("CreateInTransaction POST - Creating transaction for WarehouseID: {WarehouseID}, ProductID: {ProductID}", dto.WarehouseId, dto.ProductId);
                    await _transactionService.CreateInTransactionAsync(dto);
                    TempData["Success"] = "Transaction completed successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CreateInTransaction POST - Error creating transaction: {Message}", ex.Message);
                    TempData["error"] = $"Error: {ex.Message}";
                }
            }
            else
            {
                _logger.LogWarning("CreateInTransaction POST - Model state invalid: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }

            await PopulateViewBagAsync(dto.SupplierID, dto.WarehouseId, dto.ProductId);
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> TransferBetweenWarehouses()
        {
            try
            {
                _logger.LogInformation("TransferBetweenWarehouses GET - Loading form");
                await PopulateViewBagAsync(requireStockForSource: true);
                return View(new CreateWarehouseTransferDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TransferBetweenWarehouses GET - Error loading form: {Message}", ex.Message);
                TempData["error"] = "An error occurred while loading the transfer form.";
                return View(new CreateWarehouseTransferDto());
            }
        }

        [HttpPost]
        public async Task<IActionResult> TransferBetweenWarehouses(CreateWarehouseTransferDto dto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("TransferBetweenWarehouses POST - Creating transfer FromWarehouseID: {FromWarehouseID}, ToWarehouseID: {ToWarehouseID}", dto.FromWarehouseId, dto.ToWarehouseId);

                    var fromProduct = await _unitOfWork.Products.GetByIdAsync(dto.FromProductId);
                    var toProduct = await _unitOfWork.Products.GetByIdAsync(dto.ToProductId);
                    if (fromProduct == null || toProduct == null)
                    {
                        _logger.LogWarning("TransferBetweenWarehouses POST - Source or destination product not found. FromProductID: {FromProductID}, ToProductID: {ToProductID}", dto.FromProductId, dto.ToProductId);
                        throw new Exception("Source or destination product not found.");
                    }

                    if (!string.Equals(fromProduct.ProductName, toProduct.ProductName, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("TransferBetweenWarehouses POST - Product name mismatch. Source: {SourceProductName}, Destination: {DestProductName}", fromProduct.ProductName, toProduct.ProductName);
                        throw new Exception("Source and destination products must have the same name.");
                    }

                    await _transactionService.TransferBetweenWarehousesAsync(dto);
                    TempData["Success"] = "Transfer completed successfully!";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "TransferBetweenWarehouses POST - Database error during transfer: {Message}", ex.Message);
                    TempData["error"] = "A database error occurred. Please try again or contact support.";
                    await PopulateViewBagAsync(null, dto.FromWarehouseId, dto.FromProductId, dto.ToWarehouseId, dto.ToProductId, requireStockForSource: true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "TransferBetweenWarehouses POST - Error creating transfer: {Message}", ex.Message);
                    TempData["error"] = ex.Message;
                    await PopulateViewBagAsync(null, dto.FromWarehouseId, dto.FromProductId, dto.ToWarehouseId, dto.ToProductId, requireStockForSource: true);
                }
            }
            else
            {
                _logger.LogWarning("TransferBetweenWarehouses POST - Model state invalid: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                await PopulateViewBagAsync(null, dto.FromWarehouseId, dto.FromProductId, dto.ToWarehouseId, dto.ToProductId, requireStockForSource: true);
            }

            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsByWarehouse(Guid warehouseId)
        {
            try
            {
                var products = await _productHelperService.GetAllProductsThatInThisWarehouse(warehouseId);
                _logger.LogInformation("GetProductsByWarehouse - Found {Count} products for WarehouseID {WarehouseID}", products.Count, warehouseId);
                return Json(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetProductsByWarehouse - Error fetching products for WarehouseID {WarehouseID}: {Message}", warehouseId, ex.Message);
                return StatusCode(500, new { error = "Error fetching products" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMatchingProducts(Guid productId, Guid toWarehouseId)
        {
            try
            {
                var matchingProducts = await _productHelperService.GetAllProductsThatMatching(productId, toWarehouseId);
                _logger.LogInformation("GetMatchingProducts - Found {Count} matching products for ProductID {ProductID} in WarehouseID {WarehouseID}", matchingProducts.Count, productId, toWarehouseId);
                return Json(matchingProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMatchingProducts - Error fetching matching products for ProductID {ProductID}, ToWarehouseID {ToWarehouseID}: {Message}", productId, toWarehouseId, ex.Message);
                return StatusCode(500, new { error = "Error fetching matching products" });
            }
        }

        private async Task PopulateViewBagAsync(Guid? selectedSupplierId = null, Guid? selectedWarehouseId = null, Guid? selectedProductId = null, Guid? selectedToWarehouseId = null, Guid? selectedToProductId = null, bool requireStockForSource = false)
        {
            try
            {
                _logger.LogInformation("PopulateViewBagAsync - Populating ViewBag for WarehouseID: {WarehouseID}, ProductID: {ProductID}", selectedWarehouseId, selectedProductId);

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
                        _logger.LogInformation("PopulateViewBagAsync - Filtered warehouses for Manager UserID {UserID}: {WarehouseCount}", userId, filteredWarehouseDtos.Count());
                        if (!filteredWarehouseDtos.Any())
                        {
                            _logger.LogWarning("PopulateViewBagAsync - No warehouses assigned to Manager with UserID: {UserID}", userId);
                            ModelState.AddModelError(string.Empty, "No warehouse assigned to this manager.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("PopulateViewBagAsync - UserID not found in claims for Manager role");
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

                if (selectedWarehouseId.HasValue)
                {
                    Expression<Func<WarehouseStock, bool>> stockPredicate = ws => ws.WarehouseID == selectedWarehouseId.Value;
                    if (requireStockForSource)
                    {
                        stockPredicate = ws => ws.WarehouseID == selectedWarehouseId.Value && ws.StockQuantity > 0;
                    }

                    var warehouseStocksQuery = await _unitOfWork.WarehouseStocks.FindAsync(stockPredicate, ws => ws.Product);

                    var availableProducts = warehouseStocksQuery
                        .Select(ws => new
                        {
                            ws.Product.ProductID,
                            DisplayText = $"{ws.Product.ProductName} (Stock: {ws.StockQuantity})"
                        })
                        .DistinctBy(p => p.ProductID)
                        .OrderBy(p => p.DisplayText)
                        .ToList();

                    if (!availableProducts.Any() && warehouseStocksQuery.Any())
                    {
                        _logger.LogWarning("PopulateViewBagAsync - Mismatch between WarehouseStock ProductIDs and Products table for WarehouseID {WarehouseID}", selectedWarehouseId.Value);
                        ModelState.AddModelError("FromProductId", "No products found for the selected warehouse due to a data mismatch.");
                    }
                    else if (!warehouseStocksQuery.Any())
                    {
                        _logger.LogWarning("PopulateViewBagAsync - No products assigned to WarehouseID {WarehouseID}", selectedWarehouseId.Value);
                        ModelState.AddModelError("FromProductId", "The selected warehouse has no products assigned.");
                    }

                    ViewBag.Products = new SelectList(
                        availableProducts,
                        "ProductID",
                        "DisplayText",
                        selectedProductId);

                    if (requireStockForSource && selectedProductId.HasValue)
                    {
                        var selectedStock = warehouseStocksQuery.FirstOrDefault(ws => ws.ProductID == selectedProductId.Value);
                        if (selectedStock == null || selectedStock.StockQuantity <= 0)
                        {
                            _logger.LogWarning("PopulateViewBagAsync - Product {ProductID} out of stock in WarehouseID {WarehouseID}. Available: {StockQuantity}", selectedProductId.Value, selectedWarehouseId.Value, selectedStock?.StockQuantity ?? 0);
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

                if (selectedToWarehouseId.HasValue && selectedProductId.HasValue)
                {
                    var fromProduct = products.FirstOrDefault(p => p.ProductID == selectedProductId.Value);
                    if (fromProduct != null)
                    {
                        var toWarehouseStocks = await _unitOfWork.WarehouseStocks
                            .FindAsync(ws => ws.WarehouseID == selectedToWarehouseId.Value, ws => ws.Product);

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
                            _logger.LogWarning("PopulateViewBagAsync - Selected destination product {ToProductID} does not match source product in WarehouseID {ToWarehouseID}", selectedToProductId.Value, selectedToWarehouseId.Value);
                            ModelState.AddModelError("ToProductId", "The selected destination product does not match the source product.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("PopulateViewBagAsync - Source product not found for ProductID: {ProductID}", selectedProductId.Value);
                        ViewBag.ToProducts = new SelectList(new List<object>(), "ProductID", "DisplayText", selectedToProductId);
                        ModelState.AddModelError("ToProductId", "Source product not found.");
                    }
                }
                else
                {
                    ViewBag.ToProducts = new SelectList(new List<object>(), "ProductID", "DisplayText", selectedToProductId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PopulateViewBagAsync - Error populating ViewBag: {Message}", ex.Message);
                throw;
            }
        }
    }
}
