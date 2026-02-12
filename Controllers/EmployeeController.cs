using Microsoft.AspNetCore.Mvc;
using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Models;
using EmployeeManagementAPI.Services;

namespace EmployeeManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeRepository _repository;
        private readonly IPasswordHasher _passwordHasher;

        public EmployeeController(EmployeeRepository repository, IPasswordHasher passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
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

            return Ok(new LoginResponse
            {
                EmployeeID = employee.EmployeeID,
                Name = employee.Name,
                Username = employee.Username,
                Role = employee.Role,
                Status = employee.Status
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Employee employee)
        {
            try
            {
                // Hash password before storage
                employee.Password = _passwordHasher.Hash(employee.Password);

                var employeeId = await _repository.RegisterEmployee(employee);
                return Ok(new { employeeId, message = "Registration successful" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var employee = await _repository.GetEmployeeById(id);
            if (employee == null)
            {
                return NotFound();
            }
            return Ok(employee);
        }


[HttpPut("{id}")]
public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request)
{
    if (!ModelState.IsValid) return BadRequest(ModelState);

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
        ModifiedBy = request.ModifiedBy
    };

    var success = await _repository.UpdateEmployee(employee);
    return success ? Ok(new { message = "Profile updated successfully" }) : StatusCode(500);
}
    }
}