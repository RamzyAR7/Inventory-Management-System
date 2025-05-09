using IMS.Application.DTOs.User;
using IMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace IMS.Application.Services.Interface
{
    public interface IUserService
    {
        Task<UserResDto> GetUserById(Guid id, bool includeManager = false);
        Task<IEnumerable<UserResDto>> GetAllUsers(bool includeManager = false);
        Task<IEnumerable<UserResDto>> GetAllEmployee(bool includeManager = false);
        Task<(IEnumerable<UserResDto> Items, int TotalCount)> GetAllUsersPaged(
            int pageNumber,
            int pageSize,
            Expression<Func<User, bool>> predicate = null,
            Expression<Func<User, object>> orderBy = null,
            bool sortDescending = false,
            params Expression<Func<User, object>>[] includeProperties);
        Task<UserResDto> CreateUser(UserReqDto userDto);
        Task<UserEditDto> UpdateUser(Guid id, UserEditDto userDto);
        Task<List<ManagerDto>> GetManagers(); // Updated
        Task DeleteUserbyId(Guid id);
    }
}
