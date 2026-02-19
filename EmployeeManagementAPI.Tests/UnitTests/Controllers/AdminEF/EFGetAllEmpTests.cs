using EmployeeManagementAPI.Controllers;
using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Models;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;

namespace EmployeeManagementAPI.Tests.UnitTests.Controllers.AdminEF
{
    public class GetAllEmployeesTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AdminEFController _controller;
        private readonly IPasswordHasher _hasher;
        private readonly SqliteConnection _connection;

        public GetAllEmployeesTests()
        {
            // Create SQLite in-memory connection
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new AppDbContext(options);
            _context.Database.EnsureCreated(); // Create tables
            _hasher = new PasswordHasher();
            _controller = new AdminEFController(_context);

            SetupAuthenticatedAdmin();
        }

        [Fact] // 1
        public async Task GetAllEmployees_ShouldReturn_AllEmployees() // happy path, get all employees when database not empty
        {
            // Arrange - seed database
            var employees = new List<Employee>
            {
                new Employee
                {
                    Name = "Employee 1",
                    Username = "emp1",
                    Password = _hasher.Hash("Password123"),
                    Status = "Active",
                    Role = "Employee",
                    Designation = "Developer",
                    CreatedAt = DateTime.UtcNow
                },
                new Employee
                {
                    Name = "Employee 2",
                    Username = "emp2",
                    Password = _hasher.Hash("Password123"),
                    Status = "Inactive",
                    Role = "Employee",
                    Designation = "Manager",
                    CreatedAt = DateTime.UtcNow
                },
                new Employee
                {
                    Name = "Admin User",
                    Username = "admin",
                    Password = _hasher.Hash("Password123"),
                    Status = "Active",
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                }
            };
            _context.Employees.AddRange(employees);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAllEmployees();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var summaries = Assert.IsAssignableFrom<List<EmployeeSummaryDto>>(okResult.Value);
            Assert.Equal(3, summaries.Count);
            Assert.Contains(summaries, s => s.Username == "emp1");
            Assert.Contains(summaries, s => s.Username == "emp2");
            Assert.Contains(summaries, s => s.Username == "admin");
        }

        [Fact] // 2
        public async Task GetAllEmployees_ShouldReturnEmptyList_WhenNoEmployeesExist() // +ve empty list when db empty
        {
            // Act
            var result = await _controller.GetAllEmployees();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var summaries = Assert.IsAssignableFrom<List<EmployeeSummaryDto>>(okResult.Value);
            Assert.Empty(summaries);
        }

        [Fact] // 3
        public async Task GetAllEmployees_ShouldIncludeAllStatuses() // +ve should return active and inactive employees
        {
            // Arrange
            var employees = new List<Employee>
            {
                new Employee
                {
                    Name = "Active User",
                    Username = "activeuser",
                    Password = _hasher.Hash("Password123"),
                    Status = "Active",
                    Role = "Employee",
                    CreatedAt = DateTime.UtcNow
                },
                new Employee
                {
                    Name = "Inactive User",
                    Username = "inactiveuser",
                    Password = _hasher.Hash("Password123"),
                    Status = "Inactive",
                    Role = "Employee",
                    CreatedAt = DateTime.UtcNow
                }
            };
            _context.Employees.AddRange(employees);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAllEmployees();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var summaries = Assert.IsAssignableFrom<List<EmployeeSummaryDto>>(okResult.Value);
            Assert.Equal(2, summaries.Count);
            Assert.Contains(summaries, s => s.Status == "Active");
            Assert.Contains(summaries, s => s.Status == "Inactive");
        }

        private void SetupAuthenticatedAdmin()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        public void Dispose()
        {
            // Clean up the database and the connection securely
            _context.Database.EnsureDeleted();
            _context.Dispose();

            _connection?.Close();
            _connection?.Dispose();
        }
    }
}