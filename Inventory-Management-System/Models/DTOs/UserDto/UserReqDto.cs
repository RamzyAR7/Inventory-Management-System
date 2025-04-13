using Inventory_Management_System.Entities;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Management_System.Models.DTOs.User
{
    public class UserReqDto : IValidatableObject
    {
        public Guid? UserID { get; set; }
        [Required(ErrorMessage = "UserName is required.")]
        [StringLength(50, ErrorMessage = "UserName cannot be longer than 50 characters.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Password cannot be longer than 100 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public UserRole Role { get; set; }

        public Guid? ManagerID { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult("UserName, Password, and Email are required.", new[] { nameof(UserName), nameof(Password), nameof(Email) });
            }

            if (Role == UserRole.Employee && ManagerID == null)
            {
                yield return new ValidationResult("ManagerID is required for Employee role.", new[] { nameof(ManagerID) });
            }

            if (Role == UserRole.Manager && ManagerID == null)
            {
                yield return new ValidationResult("ManagerID is required for Manager role and must be an Admin.", new[] { nameof(ManagerID) });
            }

            if (Role == UserRole.Admin && ManagerID != null)
            {
                yield return new ValidationResult("Admin role should not have a ManagerID.", new[] { nameof(ManagerID) });
            }
        }
    }
}