using AutoMapper;
using IMS.BLL.DTOs.Supplier;
using IMS.BLL.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Web.Controllers
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
        public async Task<IActionResult> Index()
        {
            var suppliers = await _supplierService.GetAllAsync();
            return View(suppliers);
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
            if (ModelState.IsValid)
            {
                await _supplierService.CreateAsync(supplier);
                TempData["success"] = "Supplier created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
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
            if (ModelState.IsValid)
            {
                await _supplierService.UpdateAsync(id, supplier);
                TempData["success"] = "Supplier edited successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
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
            await _supplierService.DeleteAsync(id);
            TempData["success"] = "Supplier deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
