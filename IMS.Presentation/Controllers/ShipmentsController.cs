﻿using AutoMapper;
using IMS.Application.DTOs.Shipment;
using IMS.Application.Services.Interface;
using IMS.Application.SharedServices.Interface;
using IMS.Domain.Entities;
using IMS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IMS.Presentation.Controllers
{
    public class ShipmentsController : Controller
    {
        private readonly IShipmentService _shipmentService;
        private readonly IDeliveryManService _deliveryManService;
        private readonly IUserService _userService;
        private readonly IShipmentHelperService _shipmentHelperService;
        private readonly IMapper _mapper;
        private readonly ILogger<ShipmentsController> _logger;

        public ShipmentsController(
            IShipmentService shipmentService,
            IMapper mapper,
            ILogger<ShipmentsController> logger,
            IDeliveryManService deliveryManService,
            IShipmentHelperService shipmentHelperService,
            IUserService userService)
        {
            _shipmentService = shipmentService;
            _mapper = mapper;
            _userService = userService;
            _shipmentHelperService = shipmentHelperService;
            _logger = logger;
            _deliveryManService = deliveryManService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 10,
            ShipmentStatus? statusFilter = null,
            string sortBy = "OrderDate",
            bool sortDescending = false)
        {
            try
            {
                _logger.LogInformation("Index - Retrieving shipments for PageNumber: {PageNumber}, PageSize: {PageSize}, StatusFilter: {StatusFilter}, SortBy: {SortBy}, SortDescending: {SortDescending}",
                    pageNumber, pageSize, statusFilter, sortBy, sortDescending);

                var (shipments, totalCount) = await _shipmentService.GetPagedShipmentsAsync(pageNumber, pageSize, statusFilter, sortBy, sortDescending);
                _logger.LogInformation("Index - Retrieved {ShipmentCount} shipments, TotalCount: {TotalCount}", shipments.Count(), totalCount);

                ViewBag.Shipments = shipments;
                ViewBag.TotalCount = totalCount;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDescending = sortDescending;
                ViewBag.ShipmentStatuses = Enum.GetValues(typeof(ShipmentStatus)).Cast<ShipmentStatus>().ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Index - Error retrieving shipments: {Message}", ex.Message);
                TempData["error"] = "An error occurred while retrieving shipments.";
                ViewBag.Shipments = new List<Shipment>();
                ViewBag.TotalCount = 0;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDescending = sortDescending;
                ViewBag.ShipmentStatuses = Enum.GetValues(typeof(ShipmentStatus)).Cast<ShipmentStatus>().ToList();
                return View();
            }
        }

        // Other actions remain unchanged
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

                var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
                if (shipment == null)
                {
                    return Json(new { success = false, message = "Shipment not found." });
                }

                await _shipmentService.UpdateShipmentStatusAsync(shipmentId, parsedStatus);

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
        public async Task<IActionResult> UpdateDeliveryMethoud(Guid id)
        {
            try
            {
                var shipment = await _shipmentService.GetShipmentByIdAsync(id);
                if (shipment == null)
                {
                    TempData["error"] = "Shipment not found.";
                    return RedirectToAction("Index");
                }

                if (shipment.Status != ShipmentStatus.Pending)
                {
                    TempData["error"] = "Shipment must be in Pending status to update delivery method.";
                    return RedirectToAction("Index");
                }

                var freeDeliveryMen = await _shipmentHelperService.GetAllFreeDeliveryMen();

                var shipmentReqDto = _mapper.Map<ShipmentReqDto>(shipment);
                ViewBag.FreeDeliveryMen = new SelectList(
                    freeDeliveryMen.Select(d => new { d.DeliveryManID, DisplayName = d.FullName }),
                    "DeliveryManID",
                    "DisplayName",
                    shipmentReqDto.DeliveryManID
                );

                return View(shipmentReqDto);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogWarning("UpdateDeliveryMethoud - Shipment not found for ShipmentID: {ShipmentID}", id);
                TempData["error"] = "Shipment not found.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateDeliveryMethoud - Error retrieving shipment: {Message}", ex.Message);
                TempData["error"] = "An error occurred while retrieving the shipment.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDeliveryMethoud(ShipmentReqDto shipmentDto)
        {
            _logger.LogInformation("UpdateDeliveryMethoud called for shipmentId: {ShipmentId}, DeliveryMethod: {DeliveryMethod}, DeliveryManID: {DeliveryManID}",
                shipmentDto.ShipmentID, shipmentDto.DeliveryMethod, shipmentDto.DeliveryManID);
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    _logger.LogWarning("Validation failed: {Errors}", string.Join(", ", errors));
                    var freeDeliveryMen = await _shipmentHelperService.GetAllFreeDeliveryMen();
                    var deliveryMethods = Enum.GetValues(typeof(DeliveryMethod)).Cast<DeliveryMethod>().ToList();

                    ViewBag.DeliveryMethods = new SelectList(deliveryMethods);
                    ViewBag.FreeDeliveryMen = new SelectList(
                        freeDeliveryMen.Select(d => new { d.DeliveryManID, DisplayName = d.FullName }),
                        "DeliveryManID",
                        "DisplayName",
                        shipmentDto.DeliveryManID
                    );
                    return View(shipmentDto);
                }

                var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentDto.ShipmentID);
                if (shipment == null)
                {
                    TempData["error"] = "Shipment not found.";
                    return RedirectToAction("Index");
                }

                await _shipmentService.UpdateDeliveryMethoud(shipmentDto);
                TempData["success"] = "Shipment delivery method updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shipment: {Message}", ex.Message);
                TempData["error"] = "An error occurred while updating the shipment.";
                return RedirectToAction("Index");
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
