using Inventory_Management_System.Models.DTOs.User;
using Microsoft.EntityFrameworkCore.Query;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface IUserService
    {
        Task<UserResDto> GetUserById(Guid id, bool includeManager = false);
        Task<UserResDto> GetUserByName(string username, bool includeManager = false);
        Task<IEnumerable<UserResDto>> GetAllUsers(bool includeManager = false);

        Task<UserResDto> CreateUser(UserReqDto userDto);
        Task<UserResDto> UpdateUser(Guid id, UserReqDto userDto);

        Task DeleteUserbyId(Guid id);
        Task DeleteUserbyName(string username);
    }
}
