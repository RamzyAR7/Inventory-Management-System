using AutoMapper;
using IMS.Application.Services.Interface;
using IMS.Application.DTOs.User;
using IMS.Infrastructure.Repositories.Implementation;
using IMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

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
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string roleFilter = "", string sortBy = "UserName", bool sortDescending = false)
        {
            try
            {
                Expression<Func<User, bool>> predicate = null;

                // Apply role-based filtering based on the user's role
                if (User.IsInRole("Manager"))
                {
                    predicate = u => u.Role == "Employee";
                }

                // Apply additional role filter if provided
                if (!string.IsNullOrEmpty(roleFilter))
                {
                    if (predicate == null)
                    {
                        predicate = u => u.Role == roleFilter;
                    }
                    else
                    {
                        predicate = predicate.And(u => u.Role == roleFilter);
                    }
                }

                // Define sorting logic
                Expression<Func<User, object>> orderBy = sortBy switch
                {
                    "Email" => u => u.Email,
                    "IsActive" => u => u.IsActive,
                    "Role" => u => u.Role,
                    _ => u => u.UserName // Default to UserName
                };

                // Fetch paged users using the service
                var (users, totalCount) = await _userService.GetAllUsersPaged(
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    predicate: predicate,
                    orderBy: orderBy,
                    sortDescending: sortDescending,
                    includeProperties: u => u.Manager);

                ViewBag.TotalCount = totalCount;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.RoleFilter = roleFilter;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDescending = sortDescending;

                return View(users);
            }
            catch (KeyNotFoundException)
            {
                TempData["error"] = "No users found.";
                ViewBag.TotalCount = 0;
                ViewBag.PageNumber = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.RoleFilter = roleFilter;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDescending = sortDescending;
                return View(new List<UserResDto>());
            }
            catch (Exception)
            {
                TempData["error"] = "An unexpected error occurred.";
                ViewBag.TotalCount = 0;
                ViewBag.PageNumber = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.RoleFilter = roleFilter;
                ViewBag.SortBy = sortBy;
                ViewBag.SortDescending = sortDescending;
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
                    TempData["error"] = "No managers or admins available. Please create an Admin or Manager first.";
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
                TempData["error"] = "An unexpected error occurred: " + ex.Message;
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
                TempData["error"] = "An unexpected error occurred: " + ex.Message;
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
            catch (InvalidOperationException ex)
            {
                TempData["error"] = ex.Message;
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
