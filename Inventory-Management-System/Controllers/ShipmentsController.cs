using AutoMapper;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Shipment;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    public class ShipmentsController : Controller
    {
        private readonly IShipmentService _shipmentService;
        private readonly IMapper _mapper;
        private readonly ILogger<ShipmentsController> _logger;

        public ShipmentsController(IShipmentService shipmentService, IMapper mapper, ILogger<ShipmentsController> logger)
        {
            _shipmentService = shipmentService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, ShipmentStatus? statusFilter = null)
        {
            try
            {
                _logger.LogInformation("Index - Retrieving shipments for PageNumber: {PageNumber}, PageSize: {PageSize}, StatusFilter: {StatusFilter}",
                    pageNumber, pageSize, statusFilter);

                var (shipments, totalCount) = await _shipmentService.GetPagedShipmentsAsync(pageNumber, pageSize, statusFilter);
                _logger.LogInformation("Index - Retrieved {ShipmentCount} shipments, TotalCount: {TotalCount}", shipments.Count(), totalCount);

                ViewBag.Shipments = shipments;
                ViewBag.TotalCount = totalCount;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.ShipmentStatuses = Enum.GetValues(typeof(ShipmentStatus)).Cast<ShipmentStatus>().ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Index - Error retrieving shipments: {Message}", ex.Message);
                TempData["error"] = "An error occurred while retrieving shipments.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var shipment = await _shipmentService.GetShipmentByIdAsync(id);
                return View(shipment);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Details - Shipment not found for ShipmentID: {ShipmentID}", id);
                TempData["error"] = "Shipment not found.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Details - Error retrieving shipment: {Message}", ex.Message);
                TempData["error"] = "An error occurred while retrieving the shipment.";
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid shipmentId, string newStatus)
        {
            try
            {
                if (!Enum.TryParse<ShipmentStatus>(newStatus, true, out var parsedStatus))
                {
                    return Json(new { success = false, message = $"Invalid status value: {newStatus}" });
                }

                // Check if the shipment exists before updating
                var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
                if (shipment == null)
                {
                    return Json(new { success = false, message = "Shipment not found." });
                }

                await _shipmentService.UpdateShipmentStatusAsync(shipmentId, parsedStatus);

                // Check if the shipment still exists after the update (e.g., deleted due to order cancellation)
                shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
                bool isDeleted = shipment == null;

                return Json(new
                {
                    success = true,
                    message = isDeleted ? "Shipment was deleted due to order cancellation." : $"Shipment status updated to {parsedStatus} successfully.",
                    isDeleted = isDeleted
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating shipment status for ID: {shipmentId}");
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var shipment = await _shipmentService.GetShipmentByIdAsync(id);
                if (shipment.Status != ShipmentStatus.Cancelled)
                {
                    TempData["error"] = "Only cancelled shipments can be deleted.";
                    return RedirectToAction("Index");
                }
                return View(shipment);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Delete - Shipment not found for ShipmentID: {ShipmentID}", id);
                TempData["error"] = "Shipment not found.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete - Error retrieving shipment: {Message}", ex.Message);
                TempData["error"] = "An error occurred while retrieving the shipment.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid shipmentId)
        {
            try
            {
                await _shipmentService.DeleteShipmentAsync(shipmentId);
                TempData["success"] = "Shipment deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Delete - Shipment not found for ShipmentID: {ShipmentID}", shipmentId);
                TempData["error"] = "Shipment not found.";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Delete - {Message}", ex.Message);
                TempData["error"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete - Error deleting shipment: {Message}", ex.Message);
                TempData["error"] = "An error occurred while deleting the shipment.";
                return RedirectToAction("Index");
            }
        }


    }
}
