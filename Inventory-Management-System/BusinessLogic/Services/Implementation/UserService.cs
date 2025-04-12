using AutoMapper;
using Inventory_Management_System.BusinessLogic.Interfaces;
using Inventory_Management_System.BusinessLogic.Services.Interface;
using Inventory_Management_System.Entities;
using Inventory_Management_System.Models.DTOs.User;

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

            // Validate ManagerID
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
            }

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.Save();

            return _mapper.Map<UserResDto>(user);
        }

        public async Task<UserResDto> UpdateUser(Guid id, UserReqDto userDto)
        {
            var existingUser = await _unitOfWork.Users.GetByIdAsync(id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException($"User with ID '{id}' not found.");
            }

            _mapper.Map(userDto, existingUser);

            // Validate ManagerID
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
            }

            await _unitOfWork.Users.UpdateAsync(existingUser);
            await _unitOfWork.Save();

            return _mapper.Map<UserResDto>(existingUser);
        }

        public async Task<IEnumerable<UserResDto>> GetAllUsers(bool includeManger = false)
        {
            var users = await _unitOfWork.Users.GetAllAsync(/*includeManger ? u => u.Manager : null*/);
            if (users == null || !users.Any())
            {
                throw new KeyNotFoundException("No users found.");
            }
            return _mapper.Map<IEnumerable<UserResDto>>(users);
        }

        public async Task<UserResDto> GetUserById(Guid id, bool includeManger = false)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id, includeManger ? u => u.Manager : null);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }
            return _mapper.Map<UserResDto>(user);
        }

        public async Task<UserResDto> GetUserByName(string username, bool includeManger = false)
        {
            var user = await _unitOfWork.Users.GetByUserNameAsync(username, includeManger ? u => u.Manager : null);
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
            var user = await _unitOfWork.Users.GetByUserNameAsync(username);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with username {username} not found.");
            }
            await _unitOfWork.Users.DeleteAsync(user.UserID);
            await _unitOfWork.Save();
        }
    }
}
