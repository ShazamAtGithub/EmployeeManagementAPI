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
    public class UpdateStatusTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AdminEFController _controller;
        private readonly IPasswordHasher _hasher;
        private readonly SqliteConnection _connection;

        public UpdateStatusTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();
            _hasher = new PasswordHasher();
            _controller = new AdminEFController(_context);

            SetupAuthenticatedAdmin("admin");
        }

        [Fact]
        public async Task UpdateEmployeeStatus_ShouldReturnOk_WhenValidRequest()
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Test Employee",
                Username = "testuser",
                Password = _hasher.Hash("Password123"),
                Status = "Active",
                Role = "Employee",
                CreatedAt = DateTime.Now
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var request = new UpdateEmployeeStatusRequest
            {
                Status = "Inactive",
                ModifiedBy = "admin"
            };

            // Act
            var result = await _controller.UpdateEmployeeStatus(employee.EmployeeID, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var updatedEmployee = await _context.Employees.FindAsync(employee.EmployeeID);
            Assert.NotNull(updatedEmployee);
            Assert.Equal("Inactive", updatedEmployee!.Status);
            Assert.Equal("admin", updatedEmployee.ModifiedBy);
            Assert.NotNull(updatedEmployee.ModifiedAt);
        }

        [Fact]
        public async Task UpdateEmployeeStatus_ShouldTrimStatusValue()
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Test Employee",
                Username = "testuser",
                Password = _hasher.Hash("Password123"),
                Status = "Active",
                Role = "Employee",
                CreatedAt = DateTime.Now
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var request = new UpdateEmployeeStatusRequest
            {
                Status = "  Inactive  ",  // with spaces
                ModifiedBy = "admin"
            };

            // Act
            var result = await _controller.UpdateEmployeeStatus(employee.EmployeeID, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var updatedEmployee = await _context.Employees.FindAsync(employee.EmployeeID);
            Assert.NotNull(updatedEmployee);
            Assert.Equal("Inactive", updatedEmployee!.Status); // trimmed
        }

        [Fact]
        public async Task UpdateEmployeeStatus_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
        {
            // Arrange
            var request = new UpdateEmployeeStatusRequest
            {
                Status = "Inactive",
                ModifiedBy = "admin"
            };

            // Act
            var result = await _controller.UpdateEmployeeStatus(999, request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateEmployeeStatus_ShouldReturnBadRequest_WhenTryingToUpdateAdminStatus()
        {
            // Arrange
            var admin = new Employee
            {
                Name = "Admin User",
                Username = "adminuser",
                Password = _hasher.Hash("Password123"),
                Status = "Active",
                Role = "Admin",
                CreatedAt = DateTime.Now
            };
            _context.Employees.Add(admin);
            await _context.SaveChangesAsync();

            var request = new UpdateEmployeeStatusRequest
            {
                Status = "Inactive",
                ModifiedBy = "admin"
            };

            // Act
            var result = await _controller.UpdateEmployeeStatus(admin.EmployeeID, request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);

            // Verify status was NOT changed
            var unchangedAdmin = await _context.Employees.FindAsync(admin.EmployeeID);
            Assert.NotNull(unchangedAdmin);
            Assert.Equal("Active", unchangedAdmin!.Status);
        }

        [Fact]
        public async Task UpdateEmployeeStatus_ShouldSetModifiedByFromUserIdentity_WhenNotProvided()
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Test Employee",
                Username = "testuser",
                Password = _hasher.Hash("Password123"),
                Status = "Active",
                Role = "Employee",
                CreatedAt = DateTime.Now
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var request = new UpdateEmployeeStatusRequest
            {
                Status = "Inactive"
                // ModifiedBy is not set
            };

            // Act
            var result = await _controller.UpdateEmployeeStatus(employee.EmployeeID, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var updatedEmployee = await _context.Employees.FindAsync(employee.EmployeeID);
            Assert.NotNull(updatedEmployee);
            Assert.Equal("admin", updatedEmployee!.ModifiedBy); // from User.Identity.Name
        }

        [Fact]
        public async Task UpdateEmployeeStatus_ShouldChangeFromInactiveToActive()
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Test Employee",
                Username = "testuser",
                Password = _hasher.Hash("Password123"),
                Status = "Inactive",
                Role = "Employee",
                CreatedAt = DateTime.Now
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var request = new UpdateEmployeeStatusRequest
            {
                Status = "Active",
                ModifiedBy = "admin"
            };

            // Act
            var result = await _controller.UpdateEmployeeStatus(employee.EmployeeID, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var updatedEmployee = await _context.Employees.FindAsync(employee.EmployeeID);
            Assert.NotNull(updatedEmployee);
            Assert.Equal("Active", updatedEmployee!.Status);
        }

        private void SetupAuthenticatedAdmin(string username)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, username),
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