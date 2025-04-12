using Inventory_Management_System.Models.DTOs.User;
using Microsoft.EntityFrameworkCore.Query;

namespace Inventory_Management_System.BusinessLogic.Services.Interface
{
    public interface IUserService
    {
        Task<UserResDto> GetUserById(Guid id, bool includeManger = false);
        Task<UserResDto> GetUserByName(string username, bool includeManger = false);
        Task<IEnumerable<UserResDto>> GetAllUsers(bool includeManger = false);

        Task<UserResDto> CreateUser(UserReqDto userDto);
        Task<UserResDto> UpdateUser(Guid id, UserReqDto userDto);

        Task DeleteUserbyId(Guid id);
        Task DeleteUserbyName(string username);
    }
}
