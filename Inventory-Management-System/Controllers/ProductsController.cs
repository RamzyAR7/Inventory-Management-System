using AutoMapper;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inventory_Management_System.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ISupplierService _supplierService;
        private readonly IMapper _mapper;

        public ProductsController(IProductService productService,ICategoryService categoryService, ISupplierService supplierService, IMapper mapper)
        {
            _productService = productService;
            _categoryService = categoryService;
            _supplierService = supplierService;
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
            var Categories = await _categoryService.GetAllCategories();
            var Suppliers = await _supplierService.GetAllAsync();
            ViewBag.Categories = new SelectList(
                Categories.Select(c => new
                {
                    c.CategoryID,
                    c.CategoryName
                }),
                "CategoryID",
                "CategoryName"
            );
            ViewBag.Suppliers = new SelectList(
                Suppliers.Select(s => new
                {
                    s.SupplierID,
                    s.SupplierName
                }),
                "SupplierID",
                "SupplierName"
            );
            return View(new ProductReqDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductReqDto product)
        {
            var Categories = await _categoryService.GetAllCategories();
            var Suppliers = await _supplierService.GetAllAsync();
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    Categories.Select(c => new
                    {
                        c.CategoryID,
                        c.CategoryName
                    }),
                    "CategoryID",
                    "CategoryName"
                );
                ViewBag.Suppliers = new SelectList(
                    Suppliers.Select(s => new
                    {
                        s.SupplierID,
                        s.SupplierName
                    }),
                    "SupplierID",
                    "SupplierName"
                );
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
                ViewBag.Categories = new SelectList(
                    Categories.Select(c => new
                    {
                        c.CategoryID,
                        c.CategoryName
                    }),
                    "CategoryID",
                    "CategoryName"
                );
                ViewBag.Suppliers = new SelectList(
                    Suppliers.Select(s => new
                    {
                        s.SupplierID,
                        s.SupplierName
                    }),
                    "SupplierID",
                    "SupplierName"
                );
                return View(product);
            }

        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var productRes = await _productService.GetByIdAsync(id);
            if (productRes == null)
                return NotFound();

            var Categories = await _categoryService.GetAllCategories();
            var Suppliers = await _supplierService.GetAllAsync();
            ViewBag.Categories = new SelectList(
                Categories.Select(c => new
                {
                    c.CategoryID,
                    c.CategoryName
                }),
                "CategoryID",
                "CategoryName",
                productRes.CategoryID
            );
            ViewBag.Suppliers = new SelectList(
                Suppliers.Select(s => new
                {
                    s.SupplierID,
                    s.SupplierName
                }),
                "SupplierID",
                "SupplierName",
                productRes.Suppliers.Select(sp => sp.SupplierID).FirstOrDefault()
            );
            var productReq = _mapper.Map<ProductReqDto>(productRes);
            return View(productReq);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProductReqDto product)
        {
            var Categories = await _categoryService.GetAllCategories();
            var Suppliers = await _supplierService.GetAllAsync();
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(
                    Categories.Select(c => new
                    {
                        c.CategoryID,
                        c.CategoryName
                    }),
                    "CategoryID",
                    "CategoryName",
                    product.CategoryID
                );
                ViewBag.Suppliers = new SelectList(
                    Suppliers.Select(s => new
                    {
                        s.SupplierID,
                        s.SupplierName
                    }),
                    "SupplierID",
                    "SupplierName",
                    product.SuppliersIDs
                );
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
                var message = ex.Message;
                ModelState.AddModelError(string.Empty, message);
                ViewBag.Categories = new SelectList(
                    Categories.Select(c => new
                    {
                        c.CategoryID,
                        c.CategoryName
                    }),
                    "CategoryID",
                    "CategoryName",
                    product.CategoryID
                );
                ViewBag.Suppliers = new SelectList(
                    Suppliers.Select(s => new
                    {
                        s.SupplierID,
                        s.SupplierName
                    }),
                    "SupplierID",
                    "SupplierName",
                    product.SuppliersIDs
                );
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
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _productService.DeleteAsync(id);
            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
