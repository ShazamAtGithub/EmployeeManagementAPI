using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.DTOs;

namespace EmployeeManagementAPI.Controllers
{
    [ApiController]
    [Route("api/admin/ef")]
    [Authorize(Roles = "Admin")]
    public class AdminEFController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminEFController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            // Select only the fields needed for the summary grid
            // This avoids loading ProfileImage binary data and Password into memory
            var summaries = await _context.Employees
                .AsNoTracking()
                .Select(e => new EmployeeSummaryDto
                {
                    EmployeeID = e.EmployeeID,
                    Name = e.Name,
                    Designation = e.Designation,
                    Address = e.Address,
                    Department = e.Department,
                    JoiningDate = e.JoiningDate,
                    Skillset = e.Skillset,
                    Username = e.Username,
                    Status = e.Status
                })
                .ToListAsync();

            return Ok(summaries);
        }

        [HttpPut("employees/{id}/status")]
        public async Task<IActionResult> UpdateEmployeeStatus(int id, [FromBody] UpdateEmployeeStatusRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeID == id);

            if (employee == null)
                return NotFound(new { message = "Employee not found." });

            if (employee.Role == "Admin")
                return BadRequest(new { message = "Action denied." });

            employee.Status = request.Status.Trim();
            employee.ModifiedBy = User.Identity?.Name ?? "UnknownAdmin";
            employee.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Status updated successfully" });
        }

        [HttpPut("employees/{id}")]
        public async Task<IActionResult> AdminUpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeID == id);

            if (employee == null)
                return NotFound(new { message = "Employee not found." });

            if (employee.Role == "Admin")
                return BadRequest(new { message = "Action denied." });

            // Only update fields admin is allowed to change
            employee.Name = request.Name;
            employee.Designation = request.Designation;
            employee.Address = request.Address;
            employee.Department = request.Department;
            employee.JoiningDate = request.JoiningDate;
            employee.Skillset = request.Skillset;
            employee.ModifiedBy = request.ModifiedBy;
            employee.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee updated successfully" });
        }
    }
}