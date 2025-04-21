using Inventory_Management_System.Entities;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    public class ShipmentsController : Controller
    {
        private readonly IShipmentService _shipmentService;

        public ShipmentsController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        public async Task<IActionResult> Index()
        {
            var shipments = await _shipmentService.GetAllAsync();
            return View(shipments);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var shipment = await _shipmentService.GetByIdAsync(id);
            if (shipment == null) return NotFound();
            return View(shipment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Shipment shipment)
        {
            if (id != shipment.ShipmentID) return BadRequest();
            if (ModelState.IsValid)
            {
                await _shipmentService.UpdateAsync(shipment);
                return RedirectToAction(nameof(Index));
            }
            return View(shipment);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var shipment = await _shipmentService.GetByIdAsync(id);
            if (shipment == null) return NotFound();
            return View(shipment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _shipmentService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shipment shipment)
        {
            if (ModelState.IsValid)
            {
                await _shipmentService.CreateAsync(shipment);
                return RedirectToAction(nameof(Index));
            }
            return View(shipment);
        }
    }
}
