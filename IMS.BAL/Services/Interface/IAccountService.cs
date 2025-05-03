
using IMS.BAL.DTOs.User;

namespace IMS.BAL.Services
{
    public interface IAccountService
    {
        Task<(bool IsSuccess, string ErrorMessage)> LoginAsync(LoginReqDto request);
        Task LogoutAsync();
    }
}
