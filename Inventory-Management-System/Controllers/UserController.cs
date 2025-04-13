using AutoMapper;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Models.DTOs;
using Inventory_Management_System.Models.DTOs.User;
using Inventory_Management_System.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userService.GetAllUsers(includeManager: true);
                return View(users);
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "No users found.";
                return View(new List<UserResDto>());
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return View(new List<UserResDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var user = await _userService.GetUserById(id, includeManager: true);
                return View(user);
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var managers = await _userService.GetManagers();
                var user = new UserWithManagers
                {
                    User = new UserReqDto(),
                    Managers = managers
                };
                if (!managers.Any())
                {
                    TempData["WarningMessage"] = "No managers or admins available. Please create an Admin or Manager first.";
                }
                return View(user);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return View(new UserWithManagers { User = new UserReqDto(), Managers = new List<ManagerDto>() });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserWithManagers model)
        {
            var userDto = model.User;
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Managers = await _userService.GetManagers();
                    return View(model);
                }

                var result = await _userService.CreateUser(userDto);
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.Managers = await _userService.GetManagers();
                return View(model);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.Managers = await _userService.GetManagers();
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                model.Managers = await _userService.GetManagers();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var user = await _userService.GetUserById(id, includeManager: false);
                var userDto = _mapper.Map<UserReqDto>(user);
                var managers = await _userService.GetManagers();
                var model = new UserWithManagers
                {
                    User = userDto,
                    Managers = managers.Where(m => m.UserID != id).ToList()
                };
                return View(model);
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UserWithManagers model)
        {
            var userDto = model.User;
            try
            {
                if (!ModelState.IsValid)
                {
                    model.Managers = await _userService.GetManagers();
                    model.Managers = model.Managers.Where(m => m.UserID != id).ToList();
                    return View(model);
                }

                var result = await _userService.UpdateUser(id, userDto);
                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.Managers = await _userService.GetManagers();
                model.Managers = model.Managers.Where(m => m.UserID != id).ToList();
                return View(model);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.Managers = await _userService.GetManagers();
                model.Managers = model.Managers.Where(m => m.UserID != id).ToList();
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                model.Managers = await _userService.GetManagers();
                model.Managers = model.Managers.Where(m => m.UserID != id).ToList();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var user = await _userService.GetUserById(id, includeManager: true);
                return View(user);
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _userService.DeleteUserbyId(id);
                TempData["SuccessMessage"] = "User deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}