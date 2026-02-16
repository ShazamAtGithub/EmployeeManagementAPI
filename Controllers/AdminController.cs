using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace EmployeeManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly EmployeeRepository _repository;

        public AdminController(EmployeeRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _repository.GetAllEmployees();
            return Ok(employees);
        }

        [HttpPut("employees/{id}/status")]
        public async Task<IActionResult> UpdateEmployeeStatus(int id, [FromBody] UpdateEmployeeStatusRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var targetEmployee = await _repository.GetEmployeeById(id);

            if (targetEmployee == null)
            {
                return NotFound(new { message = "Employee not found." });
            }

            if (targetEmployee.Role == "Admin")
            {
                return BadRequest(new { message = "Action denied." });
            }
            var success = await _repository.UpdateEmployeeStatus(id, request.Status.Trim(), request.ModifiedBy);

            if (!success)
                return NotFound();

            return Ok(new { message = "Status updated successfully" });
        }
        [HttpPut("employees/{id}")]
        public async Task<IActionResult> AdminUpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var targetEmployee = await _repository.GetEmployeeById(id);
            if (targetEmployee == null) return NotFound(new { message = "Employee not found." });

            if (targetEmployee.Role == "Admin")
            {
                return BadRequest(new { message = "Action denied" });
            }

            // new Employee instance 
            // only map fields that the admin is allowed to change
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
            return success ? Ok(new { message = "Employee updated successfully" }) : StatusCode(500);
        }
    }

}
