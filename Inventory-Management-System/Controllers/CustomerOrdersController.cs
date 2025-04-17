using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    public class CustomerOrdersController : Controller
    {
        private readonly ICustomerOrderService _customerOrderService;

        public CustomerOrdersController(ICustomerOrderService customerOrderService)
        {
            _customerOrderService = customerOrderService;
        }

        public async Task<IActionResult> Index()
        {
            var customerOrders = await _customerOrderService.GetAllAsync();
            return View(customerOrders);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var order = await _customerOrderService.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerOrder customerOrder)
        {
            if (ModelState.IsValid)
            {
                await _customerOrderService.CreateAsync(customerOrder);
                return RedirectToAction(nameof(Index));
            }
            return View(customerOrder);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var customerOrder = await _customerOrderService.GetByIdAsync(id);
            if (customerOrder == null)
                return NotFound();

            return View(customerOrder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CustomerOrder customerOrder)
        {
            if (id != customerOrder.CustomerOrderID)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _customerOrderService.UpdateAsync(customerOrder);
                return RedirectToAction(nameof(Index));
            }
            return View(customerOrder);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var customerOrder = await _customerOrderService.GetByIdAsync(id);
            if (customerOrder == null)
                return NotFound();

            return View(customerOrder);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _customerOrderService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
