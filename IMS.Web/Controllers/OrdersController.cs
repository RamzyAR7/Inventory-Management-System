using AutoMapper;
using IMS.BLL.DTOs.Order.Request;
using IMS.BLL.DTOs.Order.Responce;
using IMS.BLL.Models;
using IMS.BLL.Services.Interface;
using IMS.BLL.SharedServices.Interface;
using IMS.DAL.UnitOfWork;
using IMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Web.Controllers
{
    [Authorize(Roles = "Admin, Manager, Employee")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IOrderHelperService _orderHelperService;
        private readonly IWhoIsUserLoginService _userLoginService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, IOrderHelperService orderHelperService, IWhoIsUserLoginService userLoginService, IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _orderHelperService = orderHelperService;
            _userLoginService = userLoginService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 10,
            OrderStatus? statusFilter = null,
            string sortBy = "OrderDate",
            bool sortDescending = false)
        {
            try
            {
                _logger.LogInformation("Index - Retrieving orders for PageNumber: {PageNumber}, PageSize: {PageSize}, StatusFilter: {StatusFilter}, SortBy: {SortBy}, SortDescending: {SortDescending}",
                    pageNumber, pageSize, statusFilter, sortBy, sortDescending);

                var (orders, totalCount) = await _orderService.GetPagedOrdersAsync(pageNumber, pageSize, statusFilter, sortBy, sortDescending);
                _logger.LogInformation("Index - Retrieved {OrderCount} orders, TotalCount: {TotalCount}", orders.Count(), totalCount);

                ViewBag.OrderStatuses = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToList();
                ViewBag.Orders = orders;
                ViewBag.TotalCount = totalCount;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDescending = sortDescending;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders.");
                TempData["error"] = "Failed to load orders: " + ex.Message;
                ViewBag.Orders = new List<OrderResponseDto>();
                ViewBag.TotalCount = 0;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDescending = sortDescending;
                ViewBag.OrderStatuses = Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>().ToList();
                return View();
            }
        }

        [HttpGet]
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
                _logger.LogInformation("Order retrieved: {OrderID}, OrderDetailsCount: {Count}", id, order.OrderDetails?.Count ?? 0);
                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order details: {OrderID}.", id);
                TempData["error"] = "Failed to load order details: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
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
                await _orderService.CreateAsync(orderDto);
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

        [HttpGet]
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
                        Quantity = od.Quantity,
                        ProductName = _unitOfWork.Products.GetByExpressionAsync(p => p.ProductID == od.ProductID).Result?.ProductName ?? "Unknown",
                        UnitPrice = _unitOfWork.Products.GetByExpressionAsync(p => p.ProductID == od.ProductID).Result?.Price ?? 0m
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
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var orderRes = await _orderService.GetByIdAsync(id);
                if (orderRes == null)
                {
                    TempData["error"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Map OrderDetailResponseDto to OrderResponseDto
                var orderResponse = _mapper.Map<OrderResponseDto>(orderRes);

                return View(orderResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete order page: {OrderID}.", id);
                TempData["error"] = "Failed to load delete order page: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _orderService.DeleteAsync(id);
                TempData["success"] = "Order deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order: {OrderID}.", id);
                TempData["error"] = "Failed to delete order: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsByWarehouseAndCategory(Guid warehouseId, Guid? categoryId)
        {
            try
            {
                var result = await _orderHelperService.GetProductsByWarehouseAndCategoryAsync(warehouseId, categoryId);

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
                var (isValid, errorMessage, orderDetail) = await _orderHelperService.ValidateAndAddProductAsync(warehouseId, productId, quantity, userId);

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

        //[HttpGet]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Recipe(Guid id)
        //{
        //    try
        //    {
        //        var order = await _orderService.GetByIdAsync(id);
        //        if (order == null)
        //        {
        //            _logger.LogWarning("Order not found: {OrderID}.", id);
        //            TempData["error"] = "Order not found.";
        //            return RedirectToAction(nameof(Index));
        //        }

        //        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(order.WarehouseID);
        //        var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerID);

        //        var orderDetailsRows = string.Join("\n", order.OrderDetails.Select(od => $@"\textit{{{od.ProductName}}} & {od.Quantity} & {od.UnitPrice}"));

        //        var latexContent = @"\documentclass[a4paper,12pt]{article}
        //                        % Setting up document class and basic configuration
        //                        \usepackage[utf8]{inputenc}
        //                        % Ensuring UTF-8 encoding for special characters
        //                        \usepackage[T1]{fontenc}
        //                        % Improved font rendering
        //                        \usepackage{geometry}
        //                        % Configuring page layout
        //                        \geometry{margin=1in}
        //                        \usepackage{booktabs}
        //                        % Professional table formatting
        //                        \usepackage{longtable}
        //                        % Support for tables spanning multiple pages
        //                        \usepackage{array}
        //                        % Enhanced table column specifications
        //                        \usepackage{colortbl}
        //                        % Adding color to tables
        //                        \usepackage{xcolor}
        //                        % Defining colors
        //                        \usepackage{fancyhdr}
        //                        % Customizing headers and footers
        //                        \pagestyle{fancy}
        //                        \fancyhf{}
        //                        \fancyhead[L]{Order Recipe}
        //                        \fancyhead[R]{Date: \today}
        //                        \fancyfoot[C]{\thepage}
        //                        \usepackage{lastpage}
        //                        % Displaying total page count
        //                        \usepackage{hyperref}
        //                        % Adding hyperlinks
        //                        \hypersetup{
        //                            colorlinks=true,
        //                            linkcolor=blue,
        //                            urlcolor=blue
        //                        }
        //                        % Configuring hyperlinks
        //                        % Font configuration at the end
        //                        \usepackage{times}
        //                        % Using Times New Roman font

        //                        \begin{document}

        //                        % Creating the title section
        //                        \begin{center}
        //                            \textbf{\Large Order Recipe Report} \\
        //                            \vspace{0.5cm}
        //                            Order ID: \textit{" + order.OrderID + @"} \\
        //                            Date Created: \textit{" + order.OrderDate.ToString("g") + @"} \\
        //                            Status: \textit{" + order.Status + @"}
        //                        \end{center}

        //                        % Adding customer information
        //                        \section*{Customer Information}
        //                        \begin{itemize}
        //                            \item Customer Name: \textit{" + customer.FullName + @"}
        //                            \item Warehouse: \textit{" + warehouse.WarehouseName + @"}
        //                        \end{itemize}

        //                        % Generating table for order details
        //                        \section*{Order Details}
        //                        \begin{longtable}{llr}
        //                            \toprule
        //                            Product & Quantity & Unit Price (\$) \\
        //                            \midrule
        //                            \endhead
        //                            \midrule
        //                            \multicolumn{3}{r}{Continued on next page} \\
        //                            \endfoot
        //                            \bottomrule
        //                            \endlastfoot
        //                            " + orderDetailsRows + @"
        //                        \end{longtable}

        //                        \end{document}";

        //        return Content(latexContent, "text/latex");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error loading recipe for order: {OrderID}.", id);
        //        TempData["error"] = "Failed to load recipe: " + ex.Message;
        //        return RedirectToAction(nameof(Index));
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> Recipe(Guid id)
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

                var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(order.WarehouseID);
                var customer = await _unitOfWork.Customers.GetByIdAsync(order.CustomerID);

                // إنشاء نموذج عرض للطباعة بدلاً من LaTeX
                var viewModel = new OrderRecipeViewModel
                {
                    OrderID = order.OrderID,
                    OrderDate = order.OrderDate,
                    // تحويل Enum إلى String
                    Status = order.Status.ToString(),
                    CustomerName = customer.FullName,
                    WarehouseName = warehouse.WarehouseName,
                    TotalAmount = order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice),
                    // تحويل OrderDetailResponseItem إلى OrderDetailViewModel
                    OrderDetails = order.OrderDetails.Select(od => new OrderDetailViewModel
                    {
                        ProductName = od.ProductName,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice
                    }).ToList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recipe for order: {OrderID}.", id);
                TempData["error"] = "Failed to load recipe: " + ex.Message;
                return RedirectToAction(nameof(Index));
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

                var accessibleWarehouseIds = await _userLoginService.GetAccessibleWarehouseIdsAsync(user.Role, userId);
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
