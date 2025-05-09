using System.ComponentModel.DataAnnotations;

namespace IMS.Application.DTOs.Customer
{
    public class CustomerReqDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }
        [Required, MaxLength(100)]
        public string PhoneNumber { get; set; }
        [Required, MaxLength(255)]
        public string Email { get; set; }
        [Required, MaxLength(15)]
        public string Address { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;
    }
}
