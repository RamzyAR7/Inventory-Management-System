using AutoMapper;
using IMS.BAL.DTOs.DeliveryMan;
using IMS.BAL.Services.Implementation;
using IMS.BAL.Services.Interface;
using IMS.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DeliveryMenController : Controller
    {
        private readonly IDeliveryManService _deliveryManService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public DeliveryMenController(IDeliveryManService deliveryManService, IUserService userService, IMapper mapper)
        {
            _deliveryManService = deliveryManService;
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var (deliveryMen, totalCount) = await _deliveryManService.GetPagedAsync(pageNumber, pageSize);
            var deliveryManDtos = _mapper.Map<IEnumerable<DeliveryMan>>(deliveryMen);

            ViewBag.TotalCount = totalCount;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;
            return View(deliveryManDtos);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var deliveryMan = await _deliveryManService.GetByIdAsync(id);
            if (deliveryMan == null)
                return NotFound();
            return View(deliveryMan);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var Managers = await _userService.GetManagers();
            if (Managers == null || !Managers.Any())
            {
                ModelState.AddModelError(string.Empty, "No managers available.");
                return View(new DeliveryManReqDto());
            }
            ViewBag.Managers = new SelectList(
                Managers.Select(m => new
                {
                    m.UserID,
                    DisplayName = $"{m.UserName} ({m.Role})"
                }),
                "UserID",
                "DisplayName"
            );
            return View(new DeliveryManReqDto());
        }
        [HttpPost]
        public async Task<IActionResult> Create(DeliveryManReqDto deliveryManDto)
        {
            var Managers = await _userService.GetManagers();
            if (!ModelState.IsValid)
            {
                ViewBag.Managers = new SelectList(
                    Managers.Select(m => new
                    {
                        m.UserID,
                        DisplayName = $"{m.UserName} ({m.Role})"
                    }),
                    "UserID",
                    "DisplayName"
                );
                return View(deliveryManDto);
            }
            try
            {
                await _deliveryManService.CreateAsync(deliveryManDto);
                TempData["Success"] = "DeliveryMan created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Managers = new SelectList(
                    Managers.Select(m => new
                    {
                        m.UserID,
                        DisplayName = $"{m.UserName} ({m.Role})"
                    }),
                    "UserID",
                    "DisplayName"
                );
                return View(deliveryManDto);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var deliveryMan = await _deliveryManService.GetByIdAsync(id);
            if (deliveryMan == null)
                return NotFound();
            var deliveryManDto = _mapper.Map<DeliveryManReqDto>(deliveryMan);
            var Managers = await _userService.GetManagers();
            ViewBag.Managers = new SelectList(
                Managers.Select(m => new
                {
                    m.UserID,
                    DisplayName = $"{m.UserName} ({m.Role})"
                }),
                "UserID",
                "DisplayName"
            );
            return View(deliveryManDto);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, DeliveryManReqDto deliveryManDto)
        {
            if (id != deliveryManDto.DeliveryManID)
                return NotFound();
            var Managers = await _userService.GetManagers();
            if (!ModelState.IsValid)
            {
                ViewBag.Managers = new SelectList(
                    Managers.Select(m => new
                    {
                        m.UserID,
                        DisplayName = $"{m.UserName} ({m.Role})"
                    }),
                    "UserID",
                    "DisplayName"
                );
                return View(deliveryManDto);
            }
            try
            {
                await _deliveryManService.UpdateAsync(id, deliveryManDto);
                TempData["Success"] = "DeliveryMan updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Managers = new SelectList(
                    Managers.Select(m => new
                    {
                        m.UserID,
                        DisplayName = $"{m.UserName} ({m.Role})"
                    }),
                    "UserID",
                    "DisplayName"
                );
                return View(deliveryManDto);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deliveryMan = await _deliveryManService.GetByIdAsync(id);
            if (deliveryMan == null)
                return NotFound();
            return View(deliveryMan);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _deliveryManService.DeleteAsync(id);
                TempData["Success"] = "DeliveryMan deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var deliveryMan = await _deliveryManService.GetByIdAsync(id); // Reload the model for the view
                return View(deliveryMan);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var deliveryMan = await _deliveryManService.GetByIdAsync(id); // Reload the model for the view
                return View(deliveryMan);
            }
        }
    }
}
