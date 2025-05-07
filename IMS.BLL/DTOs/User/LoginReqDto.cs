using System.ComponentModel.DataAnnotations;

namespace IMS.BLL.DTOs.User
{
    public class LoginReqDto
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
