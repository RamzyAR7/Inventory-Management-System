using AutoMapper;
using Inventory_Management_System.BusinessLogic.Encrypt;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.User;
using Inventory_Management_System.Models.DTOs.UserDto;
using System.Linq.Expressions;

namespace Inventory_Management_System.BusinessLogic.Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
                var manager = await _unitOfWork.Users.GetByIdAsync(currentManagerId);
                if (manager == null)
                {
                    throw new InvalidOperationException("Selected manager does not exist.");
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
                var manager = await _unitOfWork.Users.GetByIdAsync(user.ManagerID.Value);
                if (manager == null)
                {
                    throw new InvalidOperationException("Selected manager does not exist.");
                }
                if (user.Role == UserRole.Manager && manager.Role != UserRole.Admin)
                {
                    throw new InvalidOperationException("Manager role requires an Admin as the manager.");
                }
                if (user.Role == UserRole.Employee && manager.Role != UserRole.Manager && manager.Role != UserRole.Admin)
                {
                    throw new InvalidOperationException("Employee role requires a Manager or Admin as the manager.");
                }

                await CheckForManagerCycle(user.UserID, user.ManagerID);
            }

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.Save();

            return _mapper.Map<UserResDto>(user);
        }

        public async Task<UserEditDto> UpdateUser(Guid id, UserEditDto userDto)
        {
            var existingUser = await _unitOfWork.Users.GetByIdAsync(id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException($"User with ID '{id}' not found.");
            }

            // Handle password separately
            if (!string.IsNullOrWhiteSpace(userDto.Password))
            {
                // Hash the new password (assuming you have a PasswordHelper for hashing)
                existingUser.HashedPassword = PasswordHelper.HashPassword(userDto.Password);
            }
            else
            {
                userDto.Password = existingUser.HashedPassword;
            }

            _mapper.Map(userDto, existingUser);

            if (existingUser.ManagerID.HasValue)
            {
                var manager = await _unitOfWork.Users.GetByIdAsync(existingUser.ManagerID.Value);
                if (manager == null)
                {
                    throw new InvalidOperationException("Selected manager does not exist.");
                }
                if (existingUser.Role == UserRole.Manager && manager.Role != UserRole.Admin)
                {
                    throw new InvalidOperationException("Manager role requires an Admin as the manager.");
                }
                if (existingUser.Role == UserRole.Employee && manager.Role != UserRole.Manager && manager.Role != UserRole.Admin)
                {
                    throw new InvalidOperationException("Employee role requires a Manager or Admin as the manager.");
                }

                await CheckForManagerCycle(id, existingUser.ManagerID);
            }

            // Update the user in the database
            await _unitOfWork.Users.UpdateAsync(existingUser);
            await _unitOfWork.Save();

            return _mapper.Map<UserEditDto>(existingUser);
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
                var managers = await _unitOfWork.Users.FindManagerAsync(u => u.Role == UserRole.Manager || u.Role == UserRole.Admin);

                // Assuming FindAsync never returns null (common in EF Core), simplify the check
                if (!managers.Any())
                {
                    return new List<ManagerDto>(); // Return empty list if no managers are found
                }

                return _mapper.Map<List<ManagerDto>>(managers);
            }
            catch (Exception ex)
            {
                // Log the error (you can use a logging framework like Serilog or ILogger)
                // For now, we'll just rethrow a more specific exception
                throw new InvalidOperationException("Failed to retrieve managers from the database.", ex);
            }
        }

        public async Task<UserResDto> GetUserById(Guid id, bool includeManager = false)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id, includeManager ? new Expression<Func<User, object>>[] { u => u.Manager } : Array.Empty<Expression<Func<User, object>>>());
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }
            return _mapper.Map<UserResDto>(user);
        }

        public async Task<UserResDto> GetUserByName(string username, bool includeManager = false)
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserName == username, includeManager ? new Expression<Func<User, object>>[] { u => u.Manager } : Array.Empty<Expression<Func<User, object>>>());
            if (user == null)
            {
                throw new KeyNotFoundException($"User with username {username} not found.");
            }
            return _mapper.Map<UserResDto>(user);
        }

        public async Task DeleteUserbyId(Guid id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }
            await _unitOfWork.Users.DeleteAsync(id);
            await _unitOfWork.Save();
        }

        public async Task DeleteUserbyName(string username)
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with username {username} not found.");
            }
            await _unitOfWork.Users.DeleteAsync(user.UserID);
            await _unitOfWork.Save();
        }
    }
}