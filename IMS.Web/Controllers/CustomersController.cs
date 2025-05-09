using AutoMapper;
using IMS.Application.DTOs.Customer;
using IMS.Application.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using IMS.Domain.Entities;

namespace IMS.Web.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;

        public CustomersController(ICustomerService customerService, IMapper mapper)
        {
            _customerService = customerService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string sortBy = "FullName", bool sortDescending = false)
        {
            try
            {
                // Define sorting logic
                Expression<Func<Customer, object>> orderBy = sortBy switch
                {
                    "PhoneNumber" => c => c.PhoneNumber,
                    "Email" => c => c.Email,
                    "Address" => c => c.Address,
                    "IsActive" => c => c.IsActive,
                    _ => c => c.FullName // Default to FullName
                };

                // Fetch paged customers using the service
                (IEnumerable<Customer> customers, int totalCount) = await _customerService.GetAllPagedAsync(
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    predicate: null,
                    orderBy: orderBy,
                    sortDescending: sortDescending,
                    includeProperties: c => c.Orders);

                ViewBag.TotalCount = totalCount;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDescending = sortDescending;

                return View(customers);
            }
            catch (Exception)
            {
                TempData["error"] = "An unexpected error occurred.";
                ViewBag.TotalCount = 0;
                ViewBag.PageNumber = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDescending = sortDescending;
                return View(new List<Customer>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerReqDto customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _customerService.CreateAsync(customer);
                    TempData["success"] = "Customer created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(customer);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
                return NotFound();
            var customerDto = _mapper.Map<CustomerReqDto>(customer);

            return View(customerDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CustomerReqDto customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _customerService.UpdateAsync(id, customer);
                    TempData["success"] = "Customer edited successfully.";
                    return RedirectToAction(nameof(Index));
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(customer);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    TempData["error"] = "Invalid customer ID.";
                    return RedirectToAction(nameof(Index));
                }

                var customer = await _customerService.GetByIdAsync(id);
                if (customer == null)
                {
                    TempData["error"] = "Customer not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(customer);
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An unexpected error occurred: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                var customer = await _customerService.GetByIdAsync(id);
                if (customer == null)
                    return NotFound();
                await _customerService.DeleteAsync(id);
                TempData["success"] = "Customer deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
