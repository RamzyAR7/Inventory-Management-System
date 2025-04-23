using AutoMapper;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Warehouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inventory_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class WarehousesController : Controller
    {
        private readonly IWarehouseService _warehouseService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public WarehousesController(IWarehouseService warehouseService, IMapper mapper, IUserService userService)
        {
            _warehouseService = warehouseService;
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var warehouses = await _warehouseService.GetAllAsync();
            var warehouseDtos = _mapper.Map<IEnumerable<WarehouseResDto>>(warehouses); // Map to DTOs
            return View(warehouseDtos);
        }
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var warehouse = await _warehouseService.GetByIdAsync(id);
            if (warehouse == null)
                return NotFound();

            return View(warehouse);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var Managers = await _userService.GetManagers();
            if (Managers == null || !Managers.Any())
            {
                ModelState.AddModelError(string.Empty, "No managers available.");
                return View(new WarehouseReqDto());
            }
            ViewBag.Managers = new SelectList(
                Managers.Select(m => new
                {
                    m.UserID,
                    DisplayName = $"{m.UserName} ({m.Role})"
                }),
                "UserID",
                "DisplayName"
            );
            return View(new WarehouseReqDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WarehouseReqDto warehouseDto)
        {
            var Managers = await _userService.GetManagers();

            if (!ModelState.IsValid)
            {
                ViewBag.Managers = new SelectList(
                    Managers.Select(m => new
                    {
                        m.UserID,
                        DisplayName = $"{m.UserName} ({m.Role})"
                    }),
                    "UserID",
                    "DisplayName"
                );
                return View(warehouseDto);
            }
            try
            {
                await _warehouseService.CreateAsync(warehouseDto);
                TempData["SuccessMessage"] = "Warehouse created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Managers = new SelectList(
                    Managers.Select(m => new
                    {
                        m.UserID,
                        DisplayName = $"{m.UserName} ({m.Role})"
                    }),
                    "UserID",
                    "DisplayName"
                );
                return View(warehouseDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var warehouseRes = await _warehouseService.GetByIdAsync(id);
            if (warehouseRes == null)
                return NotFound();
            var warehouse = _mapper.Map<WarehouseReqDto>(warehouseRes);

            var Managers = await _userService.GetManagers();
            ViewBag.Managers = new SelectList(
                Managers.Select(m => new
                {
                    m.UserID,
                    DisplayName = $"{m.UserName} ({m.Role})"
                }),
                "UserID",
                "DisplayName",
                warehouse.ManagerID // Set the selected value
            );
            return View(warehouse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, WarehouseReqDto warehouse)
        {
            if (!ModelState.IsValid)
            {
                var Managers = await _userService.GetManagers();
                ViewBag.Managers = new SelectList(
                    Managers.Select(m => new
                    {
                        m.UserID,
                        DisplayName = $"{m.UserName} ({m.Role})"
                    }),
                    "UserID",
                    "DisplayName",
                    warehouse.ManagerID // Set the selected value
                );
                return View(warehouse);
            }
            try
            {
                await _warehouseService.UpdateAsync(id, warehouse);
                TempData["SuccessMessage"] = "Warehouse updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            return View(warehouse);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var warehouse = await _warehouseService.GetByIdAsync(id);
                if (warehouse == null)
                    return NotFound();

                return View(warehouse);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var warehouse = await _warehouseService.GetByIdAsync(id);
                if (warehouse == null)
                    return NotFound();
                // Check if the warehouse has any associated stocks or shipments
                if (warehouse.WarehouseStocks.Any() || warehouse.Shipments.Any())
                {
                    ModelState.AddModelError(string.Empty, "Cannot delete a warehouse with associated stocks or shipments.");
                    return View(warehouse);
                }
                await _warehouseService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Warehouse deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
