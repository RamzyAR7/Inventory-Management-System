using IMS.BLL.SharedServices.Interface;
using IMS.DAL.Entities;
using IMS.DAL.UnitOfWork;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IMS.BLL.SharedServices.Impelimentation
{
    public class WhoIsUserloginService: IWhoIsUserLoginService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<WhoIsUserloginService> _logger;
        public WhoIsUserloginService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILogger<WhoIsUserloginService> logger)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<string> GetCurrentUserId()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                _logger.LogError("GetCurrentUserId - HttpContext is null. User ID cannot be determined.");
                throw new InvalidOperationException("HttpContext is not available. This operation requires an active HTTP request.");
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("GetCurrentUserId - User ID claim is missing.");
                throw new InvalidOperationException("User not authenticated.");
            }

            return userId;
        }
        public async Task<string> GetCurrentUserRole()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                _logger.LogError("GetCurrentUserRole - HttpContext is null. User role cannot be determined.");
                throw new InvalidOperationException("HttpContext is not available. This operation requires an active HTTP request.");
            }

            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("GetCurrentUserRole - User ID claim is missing.");
                throw new InvalidOperationException("User not authenticated.");
            }

            var user = await _unitOfWork.Users.GetByExpressionAsync(e => e.UserID == Guid.Parse(userId));
            if (user == null)
            {
                _logger.LogError("GetCurrentUserRole - User not found in the database. UserID: {UserID}", userId);
                throw new InvalidOperationException("User not found.");
            }

            return user.Role;
        }
        public async Task<List<Guid>> GetAccessibleWarehouseIdsAsync(string role, Guid userId)
        {
            if (role == "Admin")
            {
                return (await _unitOfWork.Warehouses.GetAllAsync()).Select(w => w.WarehouseID).ToList();
            }

            if (role == "Manager")
            {
                var warehouses = await _unitOfWork.Warehouses.FindAsync(w => w.ManagerID == userId);
                return warehouses.Select(w => w.WarehouseID).ToList();
            }

            if (role == "Employee")
            {
                var employee = await _unitOfWork.Users.GetByIdAsync(userId);
                if (employee == null)
                    throw new InvalidOperationException("User not found.");

                if (employee.ManagerID.HasValue)
                {
                    var managerWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.ManagerID == employee.ManagerID.Value);
                    return managerWarehouses.Select(w => w.WarehouseID).ToList();
                }

                var managers = await _unitOfWork.Users.FindAsync(u => u.Role == "Manager");
                var allManagerWarehouses = new List<Warehouse>();

                foreach (var manager in managers)
                {
                    var warehouses = await _unitOfWork.Warehouses.FindAsync(w => w.ManagerID == manager.UserID);
                    allManagerWarehouses.AddRange(warehouses);
                }

                return allManagerWarehouses.Select(w => w.WarehouseID).Distinct().ToList();
            }

            return new List<Guid>();
        }
    }
}
