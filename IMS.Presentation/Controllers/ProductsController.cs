using AutoMapper;
using IMS.Application.DTOs.Products;
using IMS.Application.DTOs.Warehouse;
using IMS.Application.Services.Interface;
using IMS.Infrastructure.UnitOfWork;
using IMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace IMS.Presentation.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;
        private readonly IWarehouseService _warehouseService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            ISupplierService supplierService,
            IWarehouseService warehouseService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
            _warehouseService = warehouseService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 10,
            Guid? categoryId = null,
            string sortBy = "ProductName",
            bool sortDescending = false)
        {
            try
            {
                var (products, totalCount) = await _productService.GetPagedProductsAsync(
                    pageNumber,
                    pageSize,
                    categoryId,
                    sortBy,
                    sortDescending);

                if (!products.Any() && pageNumber == 1)
                {
                    TempData["InfoMessage"] = "No products available for the warehouses you manage.";
                }

                ViewBag.Categories = (await _categoryService.GetAllCategories()).Select(c => new SelectListItem
                {
                    Value = c.CategoryID.ToString(),
                    Text = c.CategoryName
                }).ToList();
                ViewBag.Products = products;
                ViewBag.TotalCount = totalCount;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.CategoryId = categoryId;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDescending = sortDescending;

                return View();
            }
            catch (Exception ex)
            {
                TempData["error"] = "Failed to load products: " + ex.Message;
                ViewBag.Products = new List<Product>();
                ViewBag.TotalCount = 0;
                ViewBag.PageNumber = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.CategoryId = categoryId;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDescending = sortDescending;
                ViewBag.Categories = (await _categoryService.GetAllCategories()).Select(c => new SelectListItem
                {
                    Value = c.CategoryID.ToString(),
                    Text = c.CategoryName
                }).ToList();
                return View();
            }
        }



        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
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
                TempData["error"] = "Failed to load this product: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
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
                TempData["success"] = "Product created successfully.";
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
                TempData["success"] = "Product updated successfully.";
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
                TempData["success"] = "Product deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
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
