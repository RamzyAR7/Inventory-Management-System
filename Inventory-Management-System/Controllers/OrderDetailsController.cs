using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    public class OrderDetailsController : Controller
    {
        private readonly IOrderDetailService _orderDetailService;

        public OrderDetailsController(IOrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService;
        }

        public async Task<IActionResult> Index()
        {
            var orderDetails = await _orderDetailService.GetAllAsync();
            return View(orderDetails);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var orderDetail = await _orderDetailService.GetByIdAsync(id);
            if (orderDetail == null)
                return NotFound();

            return View(orderDetail);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderDetail orderDetail)
        {
            if (ModelState.IsValid)
            {
                await _orderDetailService.CreateAsync(orderDetail);
                return RedirectToAction(nameof(Index));
            }
            return View(orderDetail);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var orderDetail = await _orderDetailService.GetByIdAsync(id);
            if (orderDetail == null)
                return NotFound();

            return View(orderDetail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailID)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _orderDetailService.UpdateAsync(orderDetail);
                return RedirectToAction(nameof(Index));
            }
            return View(orderDetail);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var orderDetail = await _orderDetailService.GetByIdAsync(id);
            if (orderDetail == null)
                return NotFound();

            return View(orderDetail);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _orderDetailService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
