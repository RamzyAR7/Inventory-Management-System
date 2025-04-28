using AutoMapper;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Inventory_Management_System.Models.DTOs.Warehouse;

namespace Inventory_Management_System.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;
        private readonly IWarehouseService _warehouseService;
        private readonly IMapper _mapper;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            ISupplierService supplierService,
            IWarehouseService warehouseService,
            IMapper mapper)
        {
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
            _warehouseService = warehouseService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateViewBagAsync();
            return View(new ProductReqDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductReqDto product)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewBagAsync(product.CategoryID, product.WarehouseIds);
                return View(product);
            }

            try
            {
                await _productService.CreateAsync(product);
                TempData["Success"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateViewBagAsync(product.CategoryID, product.WarehouseIds);
                return View(product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            var productReq = _mapper.Map<ProductReqDto>(product);
            await PopulateViewBagAsync(product.CategoryID, productReq.WarehouseIds);
            return View(productReq);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProductReqDto product)
        {
            if (!ModelState.IsValid)
            {
                await PopulateViewBagAsync(product.CategoryID, product.WarehouseIds);
                return View(product);
            }

            try
            {
                await _productService.UpdateAsync(id, product);
                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateViewBagAsync(product.CategoryID, product.WarehouseIds);
                return View(product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound();
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _productService.DeleteAsync(id);
                TempData["Success"] = "Product deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private async Task PopulateViewBagAsync(Guid? selectedCategoryId = null, List<Guid>? selectedWarehouseIds = null)
        {
            var categories = await _categoryService.GetAllCategories();
            var warehouseDtos = await _warehouseService.GetAllAsync();
            IEnumerable<WarehouseResDto> filteredWarehouseDtos = warehouseDtos;

            if (User.IsInRole("Manager"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId != null)
                {
                    filteredWarehouseDtos = warehouseDtos.Where(w => w.ManagerID == Guid.Parse(userId)).ToList();
                    if (!filteredWarehouseDtos.Any())
                    {
                        ModelState.AddModelError(string.Empty, "No warehouse assigned to this manager.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                    filteredWarehouseDtos = new List<WarehouseResDto>();
                }
            }

            ViewBag.Categories = new SelectList(
                categories.Select(c => new { c.CategoryID, c.CategoryName }),
                "CategoryID",
                "CategoryName",
                selectedCategoryId);

            ViewBag.Warehouses = filteredWarehouseDtos.Select(w => new SelectListItem
            {
                Value = w.WarehouseID.ToString(),
                Text = w.WarehouseName,
                Selected = selectedWarehouseIds != null && selectedWarehouseIds.Contains(w.WarehouseID)
            }).ToList();
        }
    }
}
