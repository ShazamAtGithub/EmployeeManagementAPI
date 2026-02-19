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
    public class UpdateEmployeeTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AdminEFController _controller;
        private readonly IPasswordHasher _hasher;
        private readonly SqliteConnection _connection;
        public UpdateEmployeeTests()
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

        [Fact]
        public async Task AdminUpdateEmployee_ShouldReturnOk_WhenValidRequest()
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Old Name",
                Username = "testuser",
                Password = _hasher.Hash("Password123"),
                Status = "Active",
                Role = "Employee",
                Designation = "Junior Dev",
                Department = "IT",
                CreatedAt = DateTime.Now
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateEmployeeRequest
            {
                Name = "New Name",
                Designation = "Senior Dev",
                Department = "Engineering",
                Address = "123 Main St",
                Skillset = "C#, React",
                ModifiedBy = "admin"
            };

            // Act
            var result = await _controller.AdminUpdateEmployee(employee.EmployeeID, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var updatedEmployee = await _context.Employees.FindAsync(employee.EmployeeID);
            Assert.Equal("New Name", updatedEmployee.Name);
            Assert.Equal("Senior Dev", updatedEmployee.Designation);
            Assert.Equal("Engineering", updatedEmployee.Department);
            Assert.Equal("123 Main St", updatedEmployee.Address);
            Assert.Equal("C#, React", updatedEmployee.Skillset);
            Assert.Equal("admin", updatedEmployee.ModifiedBy);
            Assert.NotNull(updatedEmployee.ModifiedAt);
        }

        [Fact]
        public async Task AdminUpdateEmployee_ShouldNotChangePasswordOrStatus()
        {
            // Arrange
            var originalPassword = _hasher.Hash("Password123");
            var employee = new Employee
            {
                Name = "Test User",
                Username = "testuser",
                Password = originalPassword,
                Status = "Active",
                Role = "Employee",
                CreatedAt = DateTime.Now
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateEmployeeRequest
            {
                Name = "Updated Name",
                ModifiedBy = "admin"
            };

            // Act
            var result = await _controller.AdminUpdateEmployee(employee.EmployeeID, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var updatedEmployee = await _context.Employees.FindAsync(employee.EmployeeID);
            Assert.Equal(originalPassword, updatedEmployee.Password); // unchanged
            Assert.Equal("Active", updatedEmployee.Status); // unchanged
            Assert.Equal("Employee", updatedEmployee.Role); // unchanged
        }

        [Fact]
        public async Task AdminUpdateEmployee_ShouldReturnNotFound_WhenEmployeeDoesNotExist()
        {
            // Arrange
            var updateRequest = new UpdateEmployeeRequest
            {
                Name = "New Name",
                ModifiedBy = "admin"
            };

            // Act
            var result = await _controller.AdminUpdateEmployee(999, updateRequest);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);
        }

        [Fact]
        public async Task AdminUpdateEmployee_ShouldReturnBadRequest_WhenTryingToUpdateAdmin()
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

            var updateRequest = new UpdateEmployeeRequest
            {
                Name = "New Admin Name",
                ModifiedBy = "admin"
            };

            // Act
            var result = await _controller.AdminUpdateEmployee(admin.EmployeeID, updateRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);

            // Verify name was NOT changed
            var unchangedAdmin = await _context.Employees.FindAsync(admin.EmployeeID);
            Assert.Equal("Admin User", unchangedAdmin.Name);
        }

        [Fact]
        public async Task AdminUpdateEmployee_ShouldUpdateJoiningDate()
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Test User",
                Username = "testuser",
                Password = _hasher.Hash("Password123"),
                Status = "Active",
                Role = "Employee",
                JoiningDate = new DateTime(2020, 1, 1),
                CreatedAt = DateTime.Now
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var newJoiningDate = new DateTime(2021, 6, 15);
            var updateRequest = new UpdateEmployeeRequest
            {
                Name = "Test User",
                JoiningDate = newJoiningDate,
                ModifiedBy = "admin"
            };

            // Act
            var result = await _controller.AdminUpdateEmployee(employee.EmployeeID, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var updatedEmployee = await _context.Employees.FindAsync(employee.EmployeeID);
            Assert.Equal(newJoiningDate, updatedEmployee.JoiningDate);
        }

        [Fact]
        public async Task AdminUpdateEmployee_ShouldHandleNullableFields()
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Test User",
                Username = "testuser",
                Password = _hasher.Hash("Password123"),
                Status = "Active",
                Role = "Employee",
                Designation = "Developer",
                Address = "Old Address",
                CreatedAt = DateTime.Now
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateEmployeeRequest
            {
                Name = "Test User",
                Designation = null, // Clear designation
                Address = null,     // Clear address
                ModifiedBy = "admin"
            };

            // Act
            var result = await _controller.AdminUpdateEmployee(employee.EmployeeID, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var updatedEmployee = await _context.Employees.FindAsync(employee.EmployeeID);
            Assert.Null(updatedEmployee.Designation);
            Assert.Null(updatedEmployee.Address);
        }

        [Fact]
        public async Task AdminUpdateEmployee_ShouldUpdateAllEditableFields()
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Old Name",
                Username = "testuser",
                Password = _hasher.Hash("Password123"),
                Status = "Active",
                Role = "Employee",
                CreatedAt = DateTime.Now
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateEmployeeRequest
            {
                Name = "New Name",
                Designation = "Tech Lead",
                Address = "456 Oak St",
                Department = "Technology",
                JoiningDate = new DateTime(2022, 3, 15),
                Skillset = "Leadership, Architecture",
                ModifiedBy = "admin"
            };

            // Act
            var result = await _controller.AdminUpdateEmployee(employee.EmployeeID, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            var updatedEmployee = await _context.Employees.FindAsync(employee.EmployeeID);
            Assert.Equal("New Name", updatedEmployee.Name);
            Assert.Equal("Tech Lead", updatedEmployee.Designation);
            Assert.Equal("456 Oak St", updatedEmployee.Address);
            Assert.Equal("Technology", updatedEmployee.Department);
            Assert.Equal(new DateTime(2022, 3, 15), updatedEmployee.JoiningDate);
            Assert.Equal("Leadership, Architecture", updatedEmployee.Skillset);
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