using AutoMapper;
using IMS.Application.DTOs.Warehouse;
using IMS.Application.Services.Interface;
using IMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;

namespace IMS.Web.Controllers
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
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string sortBy = "WarehouseName", bool sortDescending = false)
        {
            try
            {
                // Define sorting logic
                Expression<Func<Warehouse, object>> orderBy = sortBy switch
                {
                    "Address" => w => w.Address,
                    "ManagerName" => w => w.Manager != null ? w.Manager.UserName : "",
                    _ => w => w.WarehouseName // Default to WarehouseName
                };

                // Fetch paged warehouses using the service with all named arguments
                (IEnumerable<WarehouseResDto> warehouses, int totalCount) = await _warehouseService.GetAllPagedAsync(
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    predicate: null,
                    orderBy: orderBy,
                    sortDescending: sortDescending,
                    includeProperties: new Expression<Func<Warehouse, object>>[] { w => w.Manager, w => w.WarehouseStocks }
                );

                ViewBag.TotalCount = totalCount;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDescending = sortDescending;

                return View(warehouses);
            }
            catch (Exception)
            {
                TempData["error"] = "An unexpected error occurred.";
                ViewBag.TotalCount = 0;
                ViewBag.PageNumber = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDescending = sortDescending;
                return View(new List<WarehouseResDto>());
            }
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
            var managers = await _userService.GetManagers();
            if (managers == null || !managers.Any())
            {
                ModelState.AddModelError(string.Empty, "No managers available.");
                return View(new WarehouseReqDto());
            }
            ViewBag.Managers = new SelectList(
                managers.Select(m => new
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
            var managers = await _userService.GetManagers();

            if (!ModelState.IsValid)
            {
                ViewBag.Managers = new SelectList(
                    managers.Select(m => new
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
                TempData["success"] = "Warehouse created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Managers = new SelectList(
                    managers.Select(m => new
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

            var managers = await _userService.GetManagers();
            ViewBag.Managers = new SelectList(
                managers.Select(m => new
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
                var managers = await _userService.GetManagers();
                ViewBag.Managers = new SelectList(
                    managers.Select(m => new
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
                TempData["success"] = "Warehouse updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(warehouse);
            }
            catch (Exception)
            {
                TempData["error"] = "An unexpected error occurred.";
                return View(warehouse);
            }
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
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = "An unexpected error occurred.";
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
                await _warehouseService.DeleteAsync(id);
                TempData["success"] = "Warehouse deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["error"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
