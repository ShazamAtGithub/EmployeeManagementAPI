using Microsoft.AspNetCore.Mvc;
using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Models;
using EmployeeManagementAPI.Services; // 1. Added this namespace

namespace EmployeeManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeRepository _repository;
        private readonly IPasswordHasher _passwordHasher; // 2. Added the Hasher field

        // 3. Inject the Hasher in the constructor
        public EmployeeController(EmployeeRepository repository, IPasswordHasher passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // STEP 1: Get the user by Username only
            // (Make sure you added 'GetEmployeeByUsername' to your Repository as discussed!)
            var employee = await _repository.GetEmployeeByUsername(request.Username);

            if (employee == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // STEP 2: Verify the password using the Service
            // We compare the Plain Text input vs the Hashed Password from DB
            if (!_passwordHasher.Verify(employee.Password, request.Password))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // STEP 3: Check if the user is Active
            if (employee.Status == "Inactive")
            {
                return Unauthorized(new { message = "Account is inactive. Please contact Admin." });
            }

            // STEP 4: Return success response
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
                // STEP 1: Hash the password before saving
                employee.Password = _passwordHasher.Hash(employee.Password);

                // STEP 2: Save to DB
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

        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _repository.GetAllEmployees();
            return Ok(employees);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Map the Request DTO to a temporary Employee object
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

            if (!success)
            {
                return NotFound();
            }

            return Ok(new { message = "Employee updated successfully" });
        }

    }
}