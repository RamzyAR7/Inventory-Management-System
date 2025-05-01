using AutoMapper;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Shipment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


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
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Index - Retrieving shipments for PageNumber: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);

                var (shipments, totalCount) = await _shipmentService.GetPagedShipmentsAsync(pageNumber, pageSize);
                _logger.LogInformation("Index - Retrieved {ShipmentCount} shipments, TotalCount: {TotalCount}", shipments.Count(), totalCount);

                ViewBag.Shipments = shipments;
                ViewBag.TotalCount = totalCount;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Index - Error retrieving shipments: {Message}", ex.Message);
                TempData["ErrorMessage"] = "An error occurred while retrieving shipments.";
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
                TempData["ErrorMessage"] = "Shipment not found.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Details - Error retrieving shipment: {Message}", ex.Message);
                TempData["ErrorMessage"] = "An error occurred while retrieving the shipment.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var shipment = await _shipmentService.GetShipmentByIdAsync(id);
                var dto = _mapper.Map<ShipmentReqDto>(shipment);

                ViewBag.ShipmentStatuses = Enum.GetValues(typeof(ShipmentStatus)).Cast<ShipmentStatus>().Select(s => new SelectListItem
                {
                    Value = s.ToString(),
                    Text = s.ToString()
                });

                return View(dto);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Edit - Shipment not found for ShipmentID: {ShipmentID}", id);
                TempData["ErrorMessage"] = "Shipment not found.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Edit - Error retrieving shipment: {Message}", ex.Message);
                TempData["ErrorMessage"] = "An error occurred while retrieving the shipment.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ShipmentReqDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.ShipmentStatuses = Enum.GetValues(typeof(ShipmentStatus)).Cast<ShipmentStatus>().Select(s => new SelectListItem
                    {
                        Value = s.ToString(),
                        Text = s.ToString()
                    });
                    return View(dto);
                }

                await _shipmentService.UpdateShipmentAsync(dto);
                TempData["SuccessMessage"] = "Shipment updated successfully.";
                return RedirectToAction("Index");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Edit - {Message}", ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                ViewBag.ShipmentStatuses = Enum.GetValues(typeof(ShipmentStatus)).Cast<ShipmentStatus>().Select(s => new SelectListItem
                {
                    Value = s.ToString(),
                    Text = s.ToString()
                });
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Edit - Error updating shipment: {Message}", ex.Message);
                TempData["ErrorMessage"] = "An error occurred while updating the shipment.";
                ViewBag.ShipmentStatuses = Enum.GetValues(typeof(ShipmentStatus)).Cast<ShipmentStatus>().Select(s => new SelectListItem
                {
                    Value = s.ToString(),
                    Text = s.ToString()
                });
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _shipmentService.DeleteShipmentAsync(id);
                TempData["SuccessMessage"] = "Shipment deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("Delete - Shipment not found for ShipmentID: {ShipmentID}", id);
                TempData["ErrorMessage"] = "Shipment not found.";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Delete - {Message}", ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete - Error deleting shipment: {Message}", ex.Message);
                TempData["ErrorMessage"] = "An error occurred while deleting the shipment.";
                return RedirectToAction("Index");
            }
        }
    }
}
