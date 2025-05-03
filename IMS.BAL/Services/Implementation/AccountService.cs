using AutoMapper;
using IMS.BAL.DTOs.User;
using IMS.BAL.Hashing;
using IMS.BAL.Services;
using IMS.Data.UnitOfWork;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> LoginAsync(LoginReqDto request)
        {
            var user = await _unitOfWork.Users.GetByUserNameAsync(request.UserName);

            if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.HashedPassword))
            {
                return (false, "User Name or Password is incorrect.");
            }

            if (user.IsActive == false)
            {
                return (false, "User is not Active");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role.ToString()) // Ensure this matches the roles in your app
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return (true, null);
        }

        public async Task LogoutAsync()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}

