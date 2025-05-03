using System.ComponentModel.DataAnnotations;

namespace IMS.BAL.DTOs.Supplier
{
    public class SupplierReqDto
    {
        [Required, MaxLength(100)]
        public string SupplierName { get; set; }
        [Required, MaxLength(15)]
        [RegularExpression(@"^\+?[0-9]{10,15}$", ErrorMessage = "Invalid Phone Number")]
        public string PhoneNumber { get; set; }
        [MaxLength(255)]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
    }
}
