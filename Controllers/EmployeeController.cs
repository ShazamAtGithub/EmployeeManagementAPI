using Microsoft.AspNetCore.Mvc;
using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Models;

namespace EmployeeManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeRepository _repository;

        public EmployeeController(EmployeeRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _repository.Login(request.Username, request.Password);
            if (result == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Employee employee)
        {
            try
            {
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
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee employee)
        {
            employee.EmployeeID = id;
            var success = await _repository.UpdateEmployee(employee);
            if (!success)
            {
                return NotFound();
            }
            return Ok(new { message = "Employee updated successfully" });
        }
    }
}