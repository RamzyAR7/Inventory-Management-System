using Inventory_Management_System.Entities;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.User
{
    public class UserResDto
    {
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? ManagerID { get; set; }
        public string ManagerName { get; set; }
    }
}
