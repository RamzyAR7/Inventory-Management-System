
using IMS.BLL.DTOs.User;

namespace IMS.BLL.Services
{
    public interface IAccountService
    {
        Task<(bool IsSuccess, string ErrorMessage)> LoginAsync(LoginReqDto request);
        Task LogoutAsync();
    }
}
