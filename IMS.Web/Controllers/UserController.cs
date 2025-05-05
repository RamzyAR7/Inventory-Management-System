using AutoMapper;
using IMS.BAL.DTOs.User;
using IMS.BAL.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace IMS.Web.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
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
                IEnumerable<UserResDto> users;
                if (User.IsInRole("Admin"))
                {
                    users = await _userService.GetAllUsers(includeManager: true);
                }
                else if (User.IsInRole("Manager"))
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
                TempData["error"] = "No users found.";
                return View(new List<UserResDto>());
            }
            catch (Exception)
            {
                TempData["error"] = "An unexpected error occurred.";
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
                TempData["error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["error"] = "An unexpected error occurred.";
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
                    // ??
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
                TempData["error"] = "An unexpected error occurred.";
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

            if (User.IsInRole("Manager") && model.Role != "Employee")
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

                // Apply ManagerID restriction only for Managers
                if (User.IsInRole("Manager") && result.ManagerID != Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value))
                {
                    ModelState.AddModelError(string.Empty, "Managers can only assign themselves as the manager for new users.");
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

                TempData["success"] = "User created successfully!";
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
                if (User.IsInRole("Manager") && user.Role != "Employee")
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
                TempData["error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (AutoMapperMappingException ex)
            {
                TempData["error"] = $"Mapping error: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["error"] = $"An unexpected error occurred: {ex.Message}";
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
                var result = await _userService.UpdateUser(id, model);

                // Apply ManagerID restriction only for Managers
                if (User.IsInRole("Manager") && result.Role != "Employee")
                {
                    ModelState.AddModelError(string.Empty, "Managers can only edit users with the Employee role.");
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

                if (User.IsInRole("Manager") && result.ManagerID != Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value))
                {
                    ModelState.AddModelError(string.Empty, "Managers can only assign themselves as the manager for users they edit.");
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

                TempData["success"] = "User updated successfully!";
                return RedirectToAction("Index");
            }
            catch (KeyNotFoundException)
            {
                TempData["error"] = "User not found.";
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
                TempData["error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["error"] = "An unexpected error occurred.";
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
                if (User.IsInRole("Manager") && user.Role != "Employee")
                {
                    return Forbid();
                }
                await _userService.DeleteUserbyId(id);

                TempData["success"] = "User deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                TempData["error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["error"] = "An unexpected error occurred.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
