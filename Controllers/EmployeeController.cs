using Microsoft.AspNetCore.Mvc;
using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Models;
using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EmployeeManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeRepository _repository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;

        public EmployeeController(EmployeeRepository repository, IPasswordHasher passwordHasher, IJwtService jwtService)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var employee = await _repository.GetEmployeeByUsername(request.Username);

            if (employee == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Verify plain text input against hashed password from DB
            if (!_passwordHasher.Verify(employee.Password, request.Password))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            if (employee.Status == "Inactive")
            {
                return Unauthorized(new { message = "Account is inactive. Please contact Admin." });
            }

            // Generate JWT token
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
                    {
                        return BadRequest(new { message = "Profile image is too large. Maximum allowed size is 2 MB." });
                    }
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
                    Username = request.Username,
                    Password = _passwordHasher.Hash(request.Password),
                    CreatedBy = request.CreatedBy
                };

                var employeeId = await _repository.RegisterEmployee(employee);
                return Ok(new { employeeId, message = "Registration successful" });
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
            // Get logged-in user ID from JWT token
            var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var loggedInUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (loggedInUserRole != "Admin" && loggedInUserId != id)
            {
                return NotFound();
            }

            var employee = await _repository.GetEmployeeById(id);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (loggedInUserId != id)
            {
                return Forbid();
            }
            var targetEmployee = await _repository.GetEmployeeById(id);
            if (targetEmployee == null || targetEmployee.Role == "Admin") return NotFound();
            if (targetEmployee.Username != request.ModifiedBy)
            {
                return NotFound(); 
            }
            // Map and Update
            var employee = new Employee
            {
                EmployeeID = id,
                Name = request.Name,
                Designation = request.Designation,
                Address = request.Address,
                Department = request.Department,
                JoiningDate = request.JoiningDate,
                Skillset = request.Skillset,
                ModifiedBy = request.ModifiedBy,
            };

            var success = await _repository.UpdateEmployee(employee);
            return success ? Ok(new { message = "Profile updated successfully" }) : StatusCode(500);
        }


        [HttpPut("{id}/image")]
        [Authorize]
        public async Task<IActionResult> UpdateProfileImage(int id, [FromBody] UpdateProfileImageRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (loggedInUserId != id)
            {
                return Forbid();
            }

            // Convert the Base64 string from the frontend back to a byte array
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
                {
                    return BadRequest(new { message = "Profile image is too large. Maximum allowed size is 2 MB." });
                }
            }

            var success = await _repository.UpdateProfileImage(id, imageBytes, request.ModifiedBy);
            return success ? Ok(new { message = "Image updated successfully" }) : StatusCode(500);
        }

        [HttpGet("{id}/image")]
        [Authorize]
        public async Task<IActionResult> GetProfileImage(int id)
        {
            var imageBytes = await _repository.GetProfileImage(id);
            if (imageBytes == null) return NotFound(new { message = "No image found" });

            // Convert back to Base64 to send to React
            var base64String = Convert.ToBase64String(imageBytes);
            return Ok(new { image = base64String });
        }
    }
}