using AutoMapper;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Supplier;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    [Authorize]
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
            return RedirectToAction(nameof(Index));
        }
    }
}
