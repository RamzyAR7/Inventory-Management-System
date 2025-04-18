using Inventory_Management_System.Entities;

namespace Inventory_Management_System.Models.DTOs.UserDto
{
    public class ManagerDto
    {
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public UserRole Role { get; set; }
    }
}
