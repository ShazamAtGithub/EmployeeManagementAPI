using Microsoft.AspNetCore.Mvc;
using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Models;

namespace EmployeeManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly EmployeeRepository _repository;

        public AdminController(EmployeeRepository repository)
        {
            _repository = repository;
        }

        // Admin-only: list all employees
        [HttpGet("employees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _repository.GetAllEmployees();
            return Ok(employees);
        }

        [HttpPut("employees/{id}/status")]
        public async Task<IActionResult> UpdateEmployeeStatus(int id, [FromBody] UpdateEmployeeStatusRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Status))
                return BadRequest(new { message = "Status is required" });

            var status = request.Status.Trim();
            if (status != "Active" && status != "Inactive")
                return BadRequest(new { message = "Invalid status. Allowed values: Active, Inactive." });

            var success = await _repository.UpdateEmployeeStatus(id, status, request.ModifiedBy);
            if (!success)
                return NotFound();

            return Ok(new { message = "Status updated successfully" });
        }
    }
}
