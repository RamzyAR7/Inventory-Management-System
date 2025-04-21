using AutoMapper;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs;
using Inventory_Management_System.Models.DTOs.User;
using Inventory_Management_System.Models.DTOs.UserDto;
using Inventory_Management_System.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
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

                IEnumerable<UserResDto> users;
                if(User.IsInRole("Admin"))
                {
                    users = await _userService.GetAllUsers(includeManager: true);
                }
                else if(User.IsInRole("Manager"))
                {
                    users = await _userService.GetAllEmployee(includeManager: true);
                }
                else
                {
                    return Forbid();
                }
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

                ViewData["Managers"] = new SelectList(
                    managers.Select(m => new
                    {
                        m.UserID,
                        DisplayName = $"{m.UserName} ({m.Role})"
                    }),
                    "UserID",
                    "DisplayName"
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
            if (!ModelState.IsValid)
            {
                var managers = await _userService.GetManagers();
                ViewData["Managers"] = new SelectList(
                    managers.Select(m => new
                    {
                        m.UserID,
                        DisplayName = $"{m.UserName} ({m.Role})"
                    }),
                    "UserID",
                    "DisplayName"
                );
                return View(model);
            }
            if (User.IsInRole("Manager") && model.Role != UserRole.Employee.ToString())
            {
                ModelState.AddModelError(string.Empty, "Managers can only create users with the Employee role.");
                var managers = await _userService.GetManagers();
                ViewData["Managers"] = new SelectList(
                    managers.Select(m => new
                    {
                        m.UserID,
                        DisplayName = $"{m.UserName} ({m.Role})"
                    }),
                    "UserID",
                    "DisplayName"
                );
                return View(model);
            }
            try
            {
                var result = await _userService.CreateUser(model);
                if (result.ManagerID != Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value))
                {
                    ModelState.AddModelError(string.Empty, "Managers can not set Admin to Employee.");
                    var managers = await _userService.GetManagers();
                    ViewData["Managers"] = new SelectList(
                        managers.Select(m => new
                        {
                            m.UserID,
                            DisplayName = $"{m.UserName} ({m.Role})"
                        }),
                        "UserID",
                        "DisplayName"
                    );
                    return View(model);
                }
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var managers = await _userService.GetManagers();
                ViewData["Managers"] = new SelectList(managers.Select(m => new { m.UserID, DisplayName = $"{m.UserName} ({m.Role})" }), "UserID", "DisplayName");
                return View(model);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var managers = await _userService.GetManagers();
                ViewData["Managers"] = new SelectList(managers.Select(m => new { m.UserID, DisplayName = $"{m.UserName} ({m.Role})" }), "UserID", "DisplayName");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred: " + ex.Message);
                var managers = await _userService.GetManagers();
                ViewData["Managers"] = new SelectList(managers.Select(m => new { m.UserID, DisplayName = $"{m.UserName} ({m.Role})" }), "UserID", "DisplayName");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var user = await _userService.GetUserById(id, includeManager: false);
                if (User.IsInRole("Manager") && user.Role != UserRole.Employee.ToString())
                {
                    return Forbid();
                }
                var userDto = _mapper.Map<UserEditDto>(user);
                var managers = await _userService.GetManagers();
                ViewData["Managers"] = new SelectList(
                    managers.Select(m => new
                    {
                        m.UserID,
                        DisplayName = $"{m.UserName} ({m.Role})"
                    }),
                    "UserID",
                    "DisplayName",
                    userDto.ManagerID // Pre-select the current manager
                );
                return View(userDto);
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (AutoMapperMappingException ex)
            {
                TempData["ErrorMessage"] = $"Mapping error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UserEditDto model)
        {
            if (!ModelState.IsValid)
            {
                var managers = await _userService.GetManagers();
                ViewData["Managers"] = new SelectList(
                    managers.Select(m => new
                    {
                        m.UserID,
                        DisplayName = $"{m.UserName} ({m.Role})"
                    }),
                    "UserID",
                    "DisplayName"
                );
                return View(model);
            }
            try
            { 
                var result = await _userService.UpdateUser(id, model); // Pass UserEditDto to service
                if (User.IsInRole("Manager") && result.Role != UserRole.Employee.ToString())
                {
                    ModelState.AddModelError(string.Empty, "Managers can only create users with the Employee role.");
                    var managers = await _userService.GetManagers();
                    ViewData["Managers"] = new SelectList(
                        managers.Select(m => new
                        {
                            m.UserID,
                            DisplayName = $"{m.UserName} ({m.Role})"
                        }),
                        "UserID",
                        "DisplayName"
                    );
                    return View(model);
                }
                if (result.ManagerID != Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value))
                {
                    ModelState.AddModelError(string.Empty, "Managers can not set Admin to Employee.");
                    var managers = await _userService.GetManagers();
                    ViewData["Managers"] = new SelectList(
                        managers.Select(m => new
                        {
                            m.UserID,
                            DisplayName = $"{m.UserName} ({m.Role})"
                        }),
                        "UserID",
                        "DisplayName"
                    );
                    return View(model);
                }
                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction("Index");
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
                ViewData["Managers"] = new SelectList(managers.Select(m => new { m.UserID, DisplayName = $"{m.UserName} ({m.Role})" }), "UserID", "DisplayName");
                return View(model);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var managers = await _userService.GetManagers();
                ViewData["Managers"] = new SelectList(managers.Select(m => new { m.UserID, DisplayName = $"{m.UserName} ({m.Role})" }), "UserID", "DisplayName");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred: " + ex.Message);
                var managers = await _userService.GetManagers();
                ViewData["Managers"] = new SelectList(managers.Select(m => new { m.UserID, DisplayName = $"{m.UserName} ({m.Role})" }), "UserID", "DisplayName");
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
                var user = await _userService.GetUserById(id);
                if (User.IsInRole("Manager") && user.Role != UserRole.Employee.ToString())
                {
                    return Forbid();
                }
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