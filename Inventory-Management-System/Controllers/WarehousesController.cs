using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    public class WarehousesController : Controller
    {
        private readonly IWarehouseService _warehouseService;

        public WarehousesController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Warehouse> warehouses = await _warehouseService.GetAllAsync();
            return View(warehouses);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            Warehouse? warehouse = await _warehouseService.GetByIdAsync(id);
            if (warehouse == null)
                return NotFound();

            return View(warehouse);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Warehouse warehouse)
        {
            if (ModelState.IsValid)
            {
                await _warehouseService.CreateAsync(warehouse);
                return RedirectToAction(nameof(Index));
            }
            return View(warehouse);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            Warehouse? warehouse = await _warehouseService.GetByIdAsync(id);
            if (warehouse == null)
                return NotFound();

            return View(warehouse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Warehouse warehouse)
        {
            if (id != warehouse.WarehouseID)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _warehouseService.UpdateAsync(warehouse);
                return RedirectToAction(nameof(Index));
            }
            return View(warehouse);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            Warehouse? warehouse = await _warehouseService.GetByIdAsync(id);
            if (warehouse == null)
                return NotFound();

            return View(warehouse);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _warehouseService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
