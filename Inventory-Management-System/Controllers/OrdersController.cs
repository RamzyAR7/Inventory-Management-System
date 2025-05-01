using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Order;
using Inventory_Management_System.Models.DTOs.Order.Request;
using Inventory_Management_System.Models.DTOs.Order.Responce;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Inventory_Management_System.Controllers
{
    [Authorize(Roles = "Admin,Manager,Employee")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, IUnitOfWork unitOfWork, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _orderService.GetAllAsync();
                ViewBag.OrderStatuses = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToList();
                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders.");
                TempData["error"] = "Failed to load orders: " + ex.Message;
                return View(new List<OrderResponseDto>());
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);
                if (order == null)
                {
                    _logger.LogWarning("Order not found: {OrderID}.", id);
                    TempData["error"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order details: {OrderID}.", id);
                TempData["error"] = "Failed to load order details: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var (hasAccess, errorMessage) = await PopulateViewBagAsync();
                if (!hasAccess)
                {
                    _logger.LogWarning("Access denied: {ErrorMessage}", errorMessage);
                    TempData["error"] = errorMessage;
                    return RedirectToAction(nameof(Index));
                }
                return View(new OrderReqDto { OrderDetails = new List<OrderDetailReqDto>() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create order page.");
                TempData["error"] = "Failed to load create order page: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderReqDto orderDto)
        {
            if (!ModelState.IsValid || orderDto.OrderDetails == null || !orderDto.OrderDetails.Any())
            {
                if (orderDto.OrderDetails == null || !orderDto.OrderDetails.Any())
                {
                    ModelState.AddModelError("", "At least one product is required.");
                }
                await PopulateViewBagAsync();
                return View(orderDto);
            }

            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                orderDto.CreatedByUserID = userId;
                await _orderService.CreateAsync(orderDto, userId);
                TempData["success"] = "Order created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Validation error creating order.");
                ModelState.AddModelError("", ex.Message);
                await PopulateViewBagAsync();
                return View(orderDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access creating order.");
                ModelState.AddModelError("", ex.Message);
                await PopulateViewBagAsync();
                return View(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order.");
                ModelState.AddModelError("", "Failed to create order: " + ex.Message);
                await PopulateViewBagAsync();
                return View(orderDto);
            }
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(id);
                if (order == null)
                {
                    _logger.LogWarning("Order not found: {OrderID}.", id);
                    TempData["error"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (order.Status != OrderStatus.Pending)
                {
                    _logger.LogWarning("Cannot edit order {OrderID} with status {Status}.", id, order.Status);
                    TempData["error"] = "Can only edit orders in Pending status.";
                    return RedirectToAction(nameof(Index));
                }

                var orderDto = new OrderReqDto
                {
                    OrderID = order.OrderID,
                    CustomerID = order.CustomerID,
                    WarehouseID = order.WarehouseID,
                    CreatedByUserID = order.CreatedByUserID,
                    OrderDetails = order.OrderDetails.Select(od => new OrderDetailReqDto
                    {
                        ProductID = od.ProductID,
                        Quantity = od.Quantity
                    }).ToList()
                };

                await PopulateViewBagAsync();
                return View(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit order page: {OrderID}.", id);
                TempData["error"] = "Failed to load edit order page: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, OrderReqDto orderDto)
        {
            if (id != orderDto.OrderID)
            {
                TempData["error"] = "Order ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                await PopulateViewBagAsync();
                return View(orderDto);
            }

            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                orderDto.CreatedByUserID = userId;
                await _orderService.EditAsync(id, orderDto);
                TempData["success"] = "Order updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Validation error editing order: {OrderID}.", id);
                TempData["error"] = ex.Message;
                await PopulateViewBagAsync();
                return View(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing order: {OrderID}.", id);
                TempData["error"] = "Failed to edit order: " + ex.Message;
                await PopulateViewBagAsync();
                return View(orderDto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, OrderStatus status)
        {
            try
            {
                await _orderService.UpdateStatusAsync(id, status);
                return Json(new { success = true, message = "Order status updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Validation error updating status: {OrderID}, Status: {Status}.", id, status);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status: {OrderID}, Status: {Status}.", id, status);
                return Json(new { success = false, message = "Failed to update order status: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsByWarehouseAndCategory(Guid warehouseId, Guid? categoryId)
        {
            try
            {
                var products = await _orderService.GetProductsByWarehouseAndCategoryAsync(warehouseId, categoryId);
                var result = products.Select(p => new
                {
                    productID = p.ProductID,
                    productName = p.ProductName,
                    categoryName = p.Category?.CategoryName ?? "N/A",
                    price = p.Price,
                    stockQuantity = _unitOfWork.WarehouseStocks
                        .FirstOrDefaultAsync(ws => ws.WarehouseID == warehouseId && ws.ProductID == p.ProductID)
                        .Result?.StockQuantity ?? 0
                }).ToList();

                return Json(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access to warehouse {WarehouseID} by user.", warehouseId);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products for WarehouseID {WarehouseID}, CategoryID {CategoryID}.", warehouseId, categoryId);
                return Json(new { success = false, message = "Failed to load products: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateAndAddProduct(Guid warehouseId, Guid productId, int quantity)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var (isValid, errorMessage, orderDetail) = await _orderService.ValidateAndAddProductAsync(warehouseId, productId, quantity, userId);

                if (!isValid)
                {
                    return Json(new { success = false, message = errorMessage });
                }

                return Json(new
                {
                    success = true,
                    product = new
                    {
                        productId = orderDetail.ProductID,
                        quantity = orderDetail.Quantity,
                        unitPrice = orderDetail.UnitPrice,
                        totalPrice = orderDetail.TotalPrice
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating product addition: ProductID {ProductID}, WarehouseID {WarehouseID}.", productId, warehouseId);
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        private async Task<(bool hasAccess, string errorMessage)> PopulateViewBagAsync()
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserID == userId);
                if (user == null)
                {
                    _logger.LogError("User not found for ID: {UserID}", userId);
                    return (false, "User not found.");
                }

                var accessibleWarehouseIds = await _orderService.GetAccessibleWarehouseIdsAsync(user.Role, userId);
                if (!accessibleWarehouseIds.Any())
                {
                    _logger.LogWarning("No accessible warehouses found for user {UserID} with role {Role}", userId, user.Role);
                    return (false, "You do not have access to any warehouses. Please contact an administrator.");
                }

                var warehouses = await _unitOfWork.Warehouses
                    .FindAsync(w => accessibleWarehouseIds.Contains(w.WarehouseID));
                ViewBag.Warehouses = new SelectList(warehouses, "WarehouseID", "WarehouseName");

                var customers = await _unitOfWork.Customers.GetAllAsync();
                ViewBag.Customers = new SelectList(customers.Where(c => c.IsActive), "CustomerID", "FullName");

                var categories = await _unitOfWork.Categories.GetAllAsync();
                ViewBag.Categories = new SelectList(categories, "CategoryID", "CategoryName");

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating ViewBag.");
                return (false, "Failed to populate dropdowns: " + ex.Message);
            }
        }
    }
}
