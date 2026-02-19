using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Models;
using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Services;

namespace EmployeeManagementAPI.Controllers
{
    [ApiController]
    [Route("api/employee/ef")]
    public class EmployeeEFController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;

        public EmployeeEFController(AppDbContext context, IPasswordHasher passwordHasher, IJwtService jwtService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Username.ToLower() == request.Username.ToLower());

            if (employee == null)
                return Unauthorized(new { message = "Invalid credentials" });

            if (!_passwordHasher.Verify(employee.Password, request.Password))
                return Unauthorized(new { message = "Invalid credentials" });

            if (employee.Status == "Inactive")
                return Unauthorized(new { message = "Account is inactive. Please contact Admin." });

            var token = _jwtService.GenerateToken(
                employee.EmployeeID,
                employee.Username,
                employee.Role
            );

            return Ok(new LoginResponse
            {
                Token = token,
                EmployeeID = employee.EmployeeID,
                Name = employee.Name,
                Username = employee.Username,
                Role = employee.Role,
                Status = employee.Status
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterEmployeeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // Check for duplicate username (case insensitive)
                bool usernameExists = await _context.Employees
                    .AnyAsync(e => e.Username.ToLower() == request.Username.ToLower());

                if (usernameExists)
                    return BadRequest(new { message = "Username already exists." });

                byte[]? imageBytes = null;
                if (!string.IsNullOrWhiteSpace(request.Base64ProfileImage))
                {
                    try
                    {
                        imageBytes = Convert.FromBase64String(request.Base64ProfileImage);
                    }
                    catch (FormatException)
                    {
                        return BadRequest(new { message = "Invalid profile image. Must be a valid Base64 string." });
                    }

                    const int maxImageBytes = 2 * 1024 * 1024;
                    if (imageBytes.Length > maxImageBytes)
                        return BadRequest(new { message = "Profile image is too large. Maximum allowed size is 2 MB." });
                }

                var employee = new Employee
                {
                    Name = request.Name,
                    Designation = request.Designation,
                    Address = request.Address,
                    Department = request.Department,
                    JoiningDate = request.JoiningDate,
                    Skillset = request.Skillset,
                    ProfileImage = imageBytes,
                    Username = request.Username.ToLower(),
                    Password = _passwordHasher.Hash(request.Password),
                    Status = "Active",
                    Role = "Employee",
                    CreatedBy = request.CreatedBy ?? "Self",
                    CreatedAt = DateTime.Now
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                return Ok(new { employeeId = employee.EmployeeID, message = "Registration successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var loggedInUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (loggedInUserRole != "Admin" && loggedInUserId != id)
                return NotFound();

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeID == id);

            if (employee == null) return NotFound();

            return Ok(employee);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (loggedInUserId != id) return Forbid();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeID == id);

            if (employee == null || employee.Role == "Admin") return NotFound();

            if (employee.Username != request.ModifiedBy) return NotFound();

            // Apply updates
            employee.Name = request.Name;
            employee.Designation = request.Designation;
            employee.Address = request.Address;
            employee.Department = request.Department;
            employee.JoiningDate = request.JoiningDate;
            employee.Skillset = request.Skillset;
            employee.ModifiedBy = request.ModifiedBy;
            employee.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated successfully" });
        }

        [HttpPut("{id}/image")]
        [Authorize]
        public async Task<IActionResult> UpdateProfileImage(int id, [FromBody] UpdateProfileImageRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (loggedInUserId != id) return Forbid();

            byte[]? imageBytes = null;
            if (!string.IsNullOrWhiteSpace(request.Base64Image))
            {
                try
                {
                    imageBytes = Convert.FromBase64String(request.Base64Image);
                }
                catch (FormatException)
                {
                    return BadRequest(new { message = "Invalid image. Must be a valid Base64 string." });
                }

                const int maxImageBytes = 2 * 1024 * 1024;
                if (imageBytes.Length > maxImageBytes)
                    return BadRequest(new { message = "Profile image is too large. Maximum allowed size is 2 MB." });
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeID == id);

            if (employee == null) return NotFound();

            employee.ProfileImage = imageBytes;
            employee.ModifiedBy = string.IsNullOrWhiteSpace(request.ModifiedBy) ? "System" : request.ModifiedBy;
            employee.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Image updated successfully" });
        }

        [HttpGet("{id}/image")]
        [Authorize]
        public async Task<IActionResult> GetProfileImage(int id)
        {
            var employee = await _context.Employees
                .AsNoTracking()
                .Select(e => new { e.EmployeeID, e.ProfileImage })
                .FirstOrDefaultAsync(e => e.EmployeeID == id);

            if (employee?.ProfileImage == null)
                return NotFound(new { message = "No image found" });

            var base64String = Convert.ToBase64String(employee.ProfileImage);
            return Ok(new { image = base64String });
        }
    }
}