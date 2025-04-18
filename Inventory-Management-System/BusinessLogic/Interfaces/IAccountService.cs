using Inventory_Management_System.Models.DTOs.UserDto;

namespace Inventory_Management_System.BusinessLogic.Interfaces
{
    public interface IAccountService
    {
        Task<(bool IsSuccess, string ErrorMessage)> LoginAsync(LoginReqDto request);
        Task LogoutAsync();
    }
}
