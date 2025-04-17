using AutoMapper;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs;
using Inventory_Management_System.Models.DTOs.User;
using Inventory_Management_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService,IMapper mapper)
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
                if (!managers.Any())
                {
                    TempData["WarningMessage"] = "No managers or admins available. Please create an Admin or Manager first.";
                }

                // Include Role in the SelectList for JavaScript filtering
                ViewData["Managers"] = new SelectList(
                    managers.Select(m => new { m.UserID, Display = $"{m.UserName} ({m.Role})", m.Role }),
                    "UserID",
                    "Display",
                    null,
                    "Role"
                );
                return View(new UserReqDto());
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserReqDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var managers = await _userService.GetManagers();
                    ViewData["Managers"] = new SelectList(managers, "UserID", "UserName");
                    return View(model);
                }

                var result = await _userService.CreateUser(model);
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var managers = await _userService.GetManagers();
                ViewData["Managers"] = new SelectList(managers, "UserID", "UserName");
                return View(model);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var managers = await _userService.GetManagers();
                ViewData["Managers"] = new SelectList(managers, "UserID", "UserName");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred: " + ex.Message);
                var managers = await _userService.GetManagers();
                ViewData["Managers"] = new SelectList(managers, "UserID", "UserName");
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
                ViewData["Managers"] = new SelectList(
                    managers.Where(m => m.UserID != id).Select(m => new { m.UserID, Display = $"{m.UserName} ({m.Role})", m.Role }),
                    "UserID",
                    "Display",
                    null,
                    "Role"
                );
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
        public async Task<IActionResult> Edit(Guid id, UserReqDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var managers = await _userService.GetManagers();
                    ViewData["Managers"] = new SelectList(managers.Where(m => m.UserID != id), "UserID", "UserName");
                    return View(model);
                }

                var result = await _userService.UpdateUser(id, model);
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
                var managers = await _userService.GetManagers();
                ViewData["Managers"] = new SelectList(managers.Where(m => m.UserID != id), "UserID", "UserName");
                return View(model);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var managers = await _userService.GetManagers();
                ViewData["Managers"] = new SelectList(managers.Where(m => m.UserID != id), "UserID", "UserName");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred: " + ex.Message);
                var managers = await _userService.GetManagers();
                ViewData["Managers"] = new SelectList(managers.Where(m => m.UserID != id), "UserID", "UserName");
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

        //[HttpGet]
        //[AllowAnonymous]
        //public IActionResult Login()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Login(string email, string password)
        //{
        //    try
        //    {
        //        var token = await _loginService.AuthenticateAndGenerateToken(email, password);
        //        Response.Cookies.Append("jwt", token, new CookieOptions
        //        {
        //            HttpOnly = true,
        //            Secure = true,
        //            SameSite = SameSiteMode.Strict,
        //            Expires = DateTimeOffset.UtcNow.AddHours(1)
        //        });
        //        TempData["SuccessMessage"] = "Login successful!";
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        ModelState.AddModelError(string.Empty, ex.Message);
        //        return View();
        //    }
        //    catch (Exception)
        //    {
        //        ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
        //        return View();
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Logout()
        //{
        //    Response.Cookies.Delete("jwt");
        //    TempData["SuccessMessage"] = "Logged out successfully!";
        //    return RedirectToAction(nameof(Login));
        //}
    }
}