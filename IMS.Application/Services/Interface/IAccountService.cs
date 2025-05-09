
using IMS.Application.DTOs.User;

namespace IMS.Application.Services
{
    public interface IAccountService
    {
        Task<(bool IsSuccess, string ErrorMessage)> LoginAsync(LoginReqDto request);
        Task LogoutAsync();
    }
}
