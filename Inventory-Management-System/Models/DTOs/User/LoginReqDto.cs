using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.UserDto
{
    public class LoginReqDto
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
