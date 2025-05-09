using AutoMapper;
using IMS.Application.DTOs.Supplier;
using IMS.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using IMS.Application.Services.Implementation;
using IMS.Domain.Entities;

namespace IMS.Presentation.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SupplierController : Controller
    {
        private readonly ISupplierService _supplierService;
        private readonly IMapper _mapper;

        public SupplierController(ISupplierService supplierService, IMapper mapper)
        {
            _supplierService = supplierService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string sortBy = "SupplierName", bool sortDescending = false)
        {
            try
            {
                // Define sorting logic
                Expression<Func<Supplier, object>> orderBy = sortBy switch
                {
                    "PhoneNumber" => s => s.PhoneNumber,
                    "Email" => s => s.Email,
                    _ => s => s.SupplierName // Default to SupplierName
                };

                // Fetch paged suppliers using the service
                (IEnumerable<Supplier> suppliers, int totalCount) = await _supplierService.GetAllPagedAsync(
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    predicate: null,
                    orderBy: orderBy,
                    sortDescending: sortDescending,
                    includeProperties: new Expression<Func<Supplier, object>>[] { s => s.SupplierProducts, s => s.InventoryTransactions }
                );

                ViewBag.TotalCount = totalCount;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDescending = sortDescending;

                return View(suppliers);
            }
            catch (Exception)
            {
                TempData["error"] = "An unexpected error occurred.";
                ViewBag.TotalCount = 0;
                ViewBag.PageNumber = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDescending = sortDescending;
                return View(new List<Supplier>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);
            if (supplier == null)
                return NotFound();

            return View(supplier);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierReqDto supplier)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _supplierService.CreateAsync(supplier);
                    TempData["success"] = "Supplier created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                return View(supplier);
            }
            catch (InvalidOperationException ex)
            {
                TempData["error"] = ex.Message;
                return View(supplier);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);
            if (supplier == null)
                return NotFound();
            var supplierReq = _mapper.Map<SupplierReqDto>(supplier);
            return View(supplierReq);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, SupplierReqDto supplier)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _supplierService.UpdateAsync(id, supplier);
                    TempData["success"] = "Supplier edited successfully.";
                    return RedirectToAction(nameof(Index));
                }
                return View(supplier);
            }
            catch (NotFoundException ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["error"] = "An unexpected error occurred.";
                return View(supplier);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var supplier = await _supplierService.GetByIdAsync(id);
            if (supplier == null)
                return NotFound();

            return View(supplier);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _supplierService.DeleteAsync(id);
                TempData["success"] = "Supplier deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
