using System.ComponentModel.DataAnnotations;
using IMS.DAL.Entities;

namespace IMS.BLL.DTOs.DeliveryMan
{
    public class DeliveryManReqDto
    {
        public Guid DeliveryManID { get; set; }
        [Required(ErrorMessage = "Name is Required")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "phone Number is requared")]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Invalid Phone Number")]
        public string PhoneNumber { get; set; }
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Status is Required")]
        public bool IsActive { get; set; }
        public DeliveryManStatus Status { get; set; } = DeliveryManStatus.Free;
        [Required(ErrorMessage = "Manager ID is Required")]
        public Guid? ManagerID { get; set; } // New property to link to a manager  
    }
}
