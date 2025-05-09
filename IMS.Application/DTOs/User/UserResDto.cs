using System.ComponentModel.DataAnnotations;

namespace IMS.Application.DTOs.User
{
    public class UserResDto
    {
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public Guid? ManagerID { get; set; }
        public string ManagerName { get; set; }
    }
}
