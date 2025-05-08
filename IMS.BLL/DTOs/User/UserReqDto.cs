using System.ComponentModel.DataAnnotations;

namespace IMS.BLL.DTOs.User
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
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!#%*?&])[A-Za-z\d@$!#%*?&]{8,}$", ErrorMessage = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; }
        [Required]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression("^(Admin|Manager|Employee)$", ErrorMessage = "Role must be Admin, Manager, or Employee.")]
        public string Role { get; set; }

        public Guid? ManagerID { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult("UserName, Password, and Email are required.", new[] { nameof(UserName), nameof(Password), nameof(Email) });
            }

            if (Role == "Employee" && ManagerID == null)
            {
                yield return new ValidationResult("ManagerID is required for Employee role.", new[] { nameof(ManagerID) });
            }

            if (Role == "Manager" && ManagerID == null)
            {
                yield return new ValidationResult("ManagerID is required for Manager role and must be an Admin.", new[] { nameof(ManagerID) });
            }

            if (Role == "Admin" && ManagerID != null)
            {
                yield return new ValidationResult("Admin role should not have a ManagerID.", new[] { nameof(ManagerID) });
            }
        }
    }
}