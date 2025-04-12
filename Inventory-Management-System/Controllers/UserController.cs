using AutoMapper;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.User;
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
            ViewBag.Managers = (await _userService.GetAllUsers())
                .Where(u => u.Role == UserRole.Manager || u.Role == UserRole.Admin)
                .OrderBy(u => u.Role)
                .Select(u => new { u.UserID, u.UserName, u.Role });
            return View(new UserReqDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserReqDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Managers = (await _userService.GetAllUsers())
                        .Where(u => u.Role == UserRole.Manager || u.Role == UserRole.Admin)
                        .OrderBy(u => u.Role)
                        .Select(u => new { u.UserID, u.UserName, u.Role });
                    return View(userDto);
                }

                var result = await _userService.CreateUser(userDto);
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Managers = (await _userService.GetAllUsers())
                    .Where(u => u.Role == UserRole.Manager || u.Role == UserRole.Admin)
                    .OrderBy(u => u.Role)
                    .Select(u => new { u.UserID, u.UserName, u.Role });
                return View(userDto);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Managers = (await _userService.GetAllUsers())
                    .Where(u => u.Role == UserRole.Manager || u.Role == UserRole.Admin)
                    .OrderBy(u => u.Role)
                    .Select(u => new { u.UserID, u.UserName, u.Role });
                return View(userDto);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                ViewBag.Managers = (await _userService.GetAllUsers())
                    .Where(u => u.Role == UserRole.Manager || u.Role == UserRole.Admin)
                    .OrderBy(u => u.Role)
                    .Select(u => new { u.UserID, u.UserName, u.Role });
                return View(userDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var user = await _userService.GetUserById(id);
                var userDto = _mapper.Map<UserReqDto>(user);
                ViewBag.Managers = (await _userService.GetAllUsers())
                    .Where(u => (u.Role == UserRole.Manager || u.Role == UserRole.Admin) && u.UserID != id)
                    .OrderBy(u => u.Role)
                    .Select(u => new { u.UserID, u.UserName, u.Role });
                return View(userDto);
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
        public async Task<IActionResult> Edit(Guid id, UserReqDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Managers = (await _userService.GetAllUsers())
                        .Where(u => (u.Role == UserRole.Manager || u.Role == UserRole.Admin) && u.UserID != id)
                        .OrderBy(u => u.Role)
                        .Select(u => new { u.UserID, u.UserName, u.Role });
                    return View(userDto);
                }

                var result = await _userService.UpdateUser(id, userDto);
                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Managers = (await _userService.GetAllUsers())
                    .Where(u => (u.Role == UserRole.Manager || u.Role == UserRole.Admin) && u.UserID != id)
                    .OrderBy(u => u.Role)
                    .Select(u => new { u.UserID, u.UserName, u.Role });
                return View(userDto);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Managers = (await _userService.GetAllUsers())
                    .Where(u => (u.Role == UserRole.Manager || u.Role == UserRole.Admin) && u.UserID != id)
                    .OrderBy(u => u.Role)
                    .Select(u => new { u.UserID, u.UserName, u.Role });
                return View(userDto);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                ViewBag.Managers = (await _userService.GetAllUsers())
                    .Where(u => (u.Role == UserRole.Manager || u.Role == UserRole.Admin) && u.UserID != id)
                    .OrderBy(u => u.Role)
                    .Select(u => new { u.UserID, u.UserName, u.Role });
                return View(userDto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var user = await _userService.GetUserById(id);
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