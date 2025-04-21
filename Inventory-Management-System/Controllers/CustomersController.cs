using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _customerService.GetAllAsync();

            return View(customers);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("FullName,Email,PhoneNumber,Address")] Customer customer)
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                await _customerService.CreateAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Customer customer)
        {
            if (id != customer.CustomerID)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _customerService.UpdateAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _customerService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
