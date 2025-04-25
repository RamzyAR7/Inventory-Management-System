using AutoMapper;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public OrdersController(IOrderService orderService, IUserService userService, IMapper mapper)
        {
            _orderService = orderService;
            _userService = userService;
            _mapper = mapper;
        }

        #region Get All
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllAsync();
            var orderResDtos = _mapper.Map<List<OrderResDto>>(orders);
            return View(orderResDtos);
        }
        #endregion

        #region Details
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            var orderResDto = _mapper.Map<OrderResDto>(order);
            return View(orderResDto);
        }
        #endregion

        #region Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderReqDto orderDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _orderService.CreateAsync(orderDto);
                    TempData["SuccessMessage"] = "Order created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(orderDto);
        }

        #endregion

        #region Edit
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            var orderReqDto = _mapper.Map<OrderReqDto>(order);
            return View(orderReqDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, OrderReqDto orderDto)
        {
            if (id == Guid.Empty)
                return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    await _orderService.UpdateAsync(id, orderDto);
                    TempData["SuccessMessage"] = "Order updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(orderDto);
        }
        #endregion

        #region Delete
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _orderService.DeleteAsync(id);
                TempData["SuccessMessage"] = "Order deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion
    }
}