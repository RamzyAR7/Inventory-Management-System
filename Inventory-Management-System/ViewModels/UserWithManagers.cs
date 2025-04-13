using Inventory_Management_System.Models.DTOs;
using Inventory_Management_System.Models.DTOs.User;

namespace Inventory_Management_System.ViewModels
{
    public class UserWithManagers
    {
        public UserReqDto User { get; set; }
        public List<ManagerDto> Managers { get; set; }
    }
}
