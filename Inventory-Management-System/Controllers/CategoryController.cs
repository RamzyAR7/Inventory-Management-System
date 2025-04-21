using AutoMapper;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Models.DTOs.Category;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.CodeDom;
using System.Threading.Tasks;

namespace Inventory_Management_System.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public CategoryController(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var Categories = await _categoryService.GetAllCategories();
            return View(Categories);
        }

        [HttpGet]
        public async Task<ActionResult> Details(Guid id)
        {
            var category = await _categoryService.GetCategoryById(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CategoryReqDto categoryDto)
        {
            if(!ModelState.IsValid)
            {
                return View(categoryDto);
            }
            try
            {
                await _categoryService.CreateCategory(categoryDto);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(categoryDto);
            }
        }
        [HttpGet]
        public async Task<ActionResult> Edit(Guid id)
        {
            var category = await _categoryService.GetCategoryById(id);
            if (category == null)
                return NotFound();
            var categoryReqDto = _mapper.Map<CategoryReqDto>(category);
            return View(categoryReqDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Guid id, CategoryReqDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return View(categoryDto);
            }
            try
            {
                await _categoryService.UpdateCategory(id, categoryDto);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(categoryDto);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Delete(Guid id)
        {
            var category = await  _categoryService.GetCategoryById(id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            await _categoryService.DeleteCategory(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
