using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementAPI.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Designation { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        public DateTime? JoiningDate { get; set; }

        [StringLength(200)]
        public string? Skillset { get; set; }

        public byte[]? ProfileImage { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required, MinLength(8), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Role { get; set; } = string.Empty;

        [StringLength(50)]
        public string? CreatedBy { get; set; }

        [StringLength(50)]
        public string? ModifiedBy { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
