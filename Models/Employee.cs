using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementAPI.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string? Designation { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        public DateTime? JoiningDate { get; set; }

        [StringLength(200)]
        public string? Skillset { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required, MinLength(8), DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, StringLength(50)]
        public string Status { get; set; }

        [Required, StringLength(50)]
        public string Role { get; set; }

        [StringLength(50)]
        public string? CreatedBy { get; set; }

        [StringLength(50)]
        public string? ModifiedBy { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }

        [Required, MinLength(8), DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public int EmployeeID { get; set; }

        public string Name { get; set; }

        public string Username { get; set; }

        public string Role { get; set; }

        public string Status { get; set; }
    }
    public class UpdateEmployeeStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public string ModifiedBy { get; set; } = string.Empty;
    }
    public class UpdateEmployeeRequest
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string? Designation { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        public DateTime? JoiningDate { get; set; }

        [StringLength(200)]
        public string? Skillset { get; set; }

        [StringLength(50)]
        public string? ModifiedBy { get; set; }
    }

}
