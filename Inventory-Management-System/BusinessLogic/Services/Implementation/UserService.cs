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
            var existingUser = _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserName == userDto.UserName || u.Email == userDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with the same username or email already exists.");
            }
            var user = _mapper.Map<User>(userDto);
            user.UserID = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.Save();

            var res = _mapper.Map<UserResDto>(user);
            return res;
        }
        public async Task<UserResDto> UpdateUser(Guid id, UserReqDto userDto)
        {
            var existingUser = _unitOfWork.Users.GetByIdAsync(id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException($"User with ID '{id}' not found.");
            }
            var user = _mapper.Map<User>(userDto);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.Save();

            var res = _mapper.Map<UserResDto>(user);
            return res;
        }
        public async Task<IEnumerable<UserResDto>> GetAllUsers(bool includeManger = false)
        {
            var users = await _unitOfWork.Users.GetAllAsync(/*includeManger ? u => u.Manager : null*/);
            if(users == null)
            {
                throw new KeyNotFoundException("No users found.");
            }
            var res = _mapper.Map<IEnumerable<UserResDto>>(users);
            return res;
        }

        public async Task<UserResDto> GetUserById(Guid id, bool includeManger = false)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id, includeManger ? u => u.Manager : null);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }
            var res = _mapper.Map<UserResDto>(user);
            return res;
        }
        public async Task<UserResDto> GetUserByName(string username, bool includeManger = false)
        {
            var user = await _unitOfWork.Users.GetByUserNameAsync(username, includeManger ? u => u.Manager : null);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with username {username} not found.");
            }
            var res = _mapper.Map<UserResDto>(user);
            return res;
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
