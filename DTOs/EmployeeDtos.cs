using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementAPI.DTOs
{
    public sealed class LoginRequest
    {
        [Required, StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required, MinLength(8), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public sealed class LoginResponse
    {
        public int EmployeeID { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }

    public sealed class RegisterEmployeeRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Designation { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        public DateTime? JoiningDate { get; set; }

        [StringLength(500)]
        public string? Skillset { get; set; }

        public string? Base64ProfileImage { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required, MinLength(8), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [StringLength(100)]
        public string? CreatedBy { get; set; }
    }

    public sealed class UpdateEmployeeStatusRequest
    {
        [Required, StringLength(20)]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Invalid status. Allowed values: Active, Inactive.")]
        public string Status { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ModifiedBy { get; set; }
    }

    public sealed class UpdateEmployeeRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Designation { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        public DateTime? JoiningDate { get; set; }

        [StringLength(500)]
        public string? Skillset { get; set; }

        [StringLength(100)]
        public string? ModifiedBy { get; set; }
    }

    public sealed class UpdateProfileImageRequest
    {
        public string? Base64Image { get; set; }

        [StringLength(100)]
        public string? ModifiedBy { get; set; }
    }
}
