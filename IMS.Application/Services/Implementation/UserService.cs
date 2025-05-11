using AutoMapper;
using IMS.Application.DTOs.User;
using IMS.Application.Hashing;
using IMS.Application.Services.Interface;
using IMS.Infrastructure.UnitOfWork;
using IMS.Domain.Entities;
using System.Linq.Expressions;
using IMS.Application.SharedServices.Interface;

namespace IMS.Application.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDashboardUpdateNotifier _dashboardUpdateNotifier;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, IDashboardUpdateNotifier dashboardUpdateNotifier)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _dashboardUpdateNotifier = dashboardUpdateNotifier;
        }

        private async Task CheckForManagerCycle(Guid userId, Guid? managerId)
        {
            if (!managerId.HasValue)
                return;

            var visited = new HashSet<Guid>();
            var currentManagerId = managerId.Value;

            while (currentManagerId != Guid.Empty)
            {
                if (visited.Contains(currentManagerId))
                {
                    throw new InvalidOperationException("Manager hierarchy contains a cycle.");
                }

                if (currentManagerId == userId)
                {
                    throw new InvalidOperationException("Manager hierarchy cannot include the user being updated.");
                }

                visited.Add(currentManagerId);
                var manager = await _unitOfWork.Users.GetByExpressionAsync(e => e.UserID == currentManagerId);
                if (manager == null)
                {
                    throw new InvalidOperationException($"Manager with ID {currentManagerId} does not exist.");
                }

                currentManagerId = manager.ManagerID ?? Guid.Empty;
            }
        }

        public async Task<UserResDto> CreateUser(UserReqDto userDto)
        {
            var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserName == userDto.UserName || u.Email == userDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with the same username or email already exists.");
            }

            var user = _mapper.Map<User>(userDto);
            user.UserID = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;
            user.HashedPassword = PasswordHelper.HashPassword(userDto.Password);

            if (user.ManagerID.HasValue)
            {
                var manager = await _unitOfWork.Users.GetByExpressionAsync(e => e.UserID == user.ManagerID.Value);
                if (manager == null)
                {
                    throw new InvalidOperationException("Selected manager does not exist.");
                }
                if (user.Role == "Manager" && manager.Role != "Admin")
                {
                    throw new InvalidOperationException("Manager role requires an Admin as the manager.");
                }
                if (user.Role == "Employee" && manager.Role != "Manager" && manager.Role != "Admin")
                {
                    throw new InvalidOperationException("Employee role requires a Manager or Admin as the manager.");
                }

                await CheckForManagerCycle(user.UserID, user.ManagerID);
            }

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveAsync();
            await _dashboardUpdateNotifier.NotifyDashboardUpdateAsync();

            return _mapper.Map<UserResDto>(user);
        }

        public async Task<UserEditDto> UpdateUser(Guid id, UserEditDto userDto)
        {
            var existingUser = await _unitOfWork.Users.GetByExpressionAsync(e => e.UserID == id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException($"User with ID '{id}' not found.");
            }

            if (!string.IsNullOrWhiteSpace(userDto.Password))
            {
                existingUser.HashedPassword = PasswordHelper.HashPassword(userDto.Password);
            }
            else
            {
                userDto.Password = existingUser.HashedPassword;
            }

            var isValid = await _unitOfWork.Users.GetByExpressionAsync(u => u.UserName == userDto.UserName && u.UserID != id);
            var isValid2 = await _unitOfWork.Users.GetByExpressionAsync(u => u.Email == userDto.Email && u.UserID != id);

            if (isValid != null)
            {
                throw new InvalidOperationException("User with the same username already exists.");
            }
            if (isValid2 != null)
            {
                throw new InvalidOperationException("User with the same email already exists.");
            }

            _mapper.Map(userDto, existingUser);

            if (existingUser.ManagerID.HasValue)
            {
                var manager = await _unitOfWork.Users.GetByExpressionAsync(e => e.UserID == existingUser.ManagerID.Value);
                if (manager == null)
                {
                    throw new InvalidOperationException("Selected manager does not exist.");
                }
                if (existingUser.Role == "Manager" && manager.Role != "Admin")
                {
                    throw new InvalidOperationException("Manager role requires an Admin as the manager.");
                }
                if (existingUser.Role == "Employee" && manager.Role != "Manager" && manager.Role != "Admin")
                {
                    throw new InvalidOperationException("Employee role requires a Manager or Admin as the manager.");
                }

                await CheckForManagerCycle(id, existingUser.ManagerID);
            }

            await _unitOfWork.Users.UpdateAsync(existingUser);
            await _unitOfWork.SaveAsync();
            await _dashboardUpdateNotifier.NotifyDashboardUpdateAsync();

            return _mapper.Map<UserEditDto>(existingUser);
        }

        public async Task<(IEnumerable<UserResDto> Items, int TotalCount)> GetAllUsersPaged(
            int pageNumber,
            int pageSize,
            Expression<Func<User, bool>> predicate = null,
            Expression<Func<User, object>> orderBy = null,
            bool sortDescending = false,
            params Expression<Func<User, object>>[] includeProperties)
        {
            var (users, totalCount) = await _unitOfWork.Users.GetPagedAsync(
                pageNumber,
                pageSize,
                predicate,
                orderBy,
                sortDescending,
                includeProperties);
            return (users.Select(u => _mapper.Map<UserResDto>(u)), totalCount);
        }

        public async Task<IEnumerable<UserResDto>> GetAllUsers(bool includeManager = false)
        {
            var users = await _unitOfWork.Users.GetAllAsync(includeManager ? new Expression<Func<User, object>>[] { u => u.Manager } : Array.Empty<Expression<Func<User, object>>>());
            if (users == null || !users.Any())
            {
                throw new KeyNotFoundException("No users found.");
            }
            return _mapper.Map<IEnumerable<UserResDto>>(users);
        }

        public async Task<List<ManagerDto>> GetManagers()
        {
            try
            {
                var managers = await _unitOfWork.Users.FindManagerAsync(u => u.Role == "Manager" || u.Role == "Admin");

                if (!managers.Any())
                {
                    return new List<ManagerDto>();
                }

                return _mapper.Map<List<ManagerDto>>(managers);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve managers from the database.", ex);
            }
        }

        public async Task<IEnumerable<UserResDto>> GetAllEmployee(bool includeManager = false)
        {
            var users = await _unitOfWork.Users.FindAsync(
                u => u.Role == "Employee",
                includeManager ? new Expression<Func<User, object>>[] { u => u.Manager } : Array.Empty<Expression<Func<User, object>>>()
            );

            if (users == null || !users.Any())
            {
                throw new KeyNotFoundException("No employees found.");
            }

            return _mapper.Map<IEnumerable<UserResDto>>(users);
        }

        public async Task<UserResDto> GetUserById(Guid id, bool includeManager = false)
        {
            var user = await _unitOfWork.Users.GetByExpressionAsync(e => e.UserID == id, includeManager ? new Expression<Func<User, object>>[] { u => u.Manager } : Array.Empty<Expression<Func<User, object>>>());
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }
            return _mapper.Map<UserResDto>(user);
        }

        public async Task DeleteUserbyId(Guid id)
        {
            var user = await _unitOfWork.Users.GetByExpressionAsync(e => e.UserID == id, e => e.ManagedWarehouses, e => e.Manager);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }

            if (user.UserName == "Admin")
            {
                throw new InvalidOperationException("Deleting the Admin user is forbidden.");
            }
            if (user.ManagedWarehouses != null && user.ManagedWarehouses.Any())
            {
                throw new InvalidOperationException("Deleting this user is not allowed because they manage warehouses, but you can deactivate them.");
            }
            if (user.Manager != null)
            {
                throw new InvalidOperationException("Deleting this user is not allowed because they manage employees, but you can deactivate them.");
            }

            await _unitOfWork.Users.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }
    }
}
