using System.ComponentModel.DataAnnotations;

namespace IMS.Application.DTOs.User
{
    public class ManagerDto
    {
        public Guid UserID { get; set; }
        public string UserName { get; set; }

        public string Role { get; set; }
    }
}
