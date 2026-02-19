using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using EmployeeManagementAPI.Controllers;
using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Services;
using EmployeeManagementAPI.Models;

namespace EmployeeManagementAPI.Tests.UnitTests.Controllers.EmployeeEF
{
    public class GetEmployeeTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IPasswordHasher> _mockHasher;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly EmployeeEFController _controller;

        public GetEmployeeTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _mockHasher = new Mock<IPasswordHasher>();
            _mockJwtService = new Mock<IJwtService>();
            _controller = new EmployeeEFController(_context, _mockHasher.Object, _mockJwtService.Object);
        }

        [Fact] // 1
        public async Task GetEmployee_ShouldReturnEmployee_When_UserRequestsOwnProfile() // +ve case when user requests their own profile
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Test User",
                Username = "testuser",
                Password = "hashed_password",
                Status = "Active",
                Role = "Employee",
                CreatedAt = DateTime.Now
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            SetupAuthenticatedUser(employee.EmployeeID, "testuser", "Employee");

            // Act
            var result = await _controller.GetEmployee(employee.EmployeeID);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEmployee = Assert.IsType<Employee>(okResult.Value);
            Assert.Equal("testuser", returnedEmployee.Username);
        }

        [Fact] // 2
        public async Task GetEmployee_ShouldReturnNotFound_When_UserTriesTo_AccessAnotherProfile() // -ve case when user tries to access another profile
        {
            // Arrange
            var employee1 = new Employee
            {
                Name = "User 1",
                Username = "user1",
                Password = "hashed_password",
                Status = "Active",
                Role = "Employee",
                CreatedAt = DateTime.Now
            };
            var employee2 = new Employee
            {
                Name = "User 2",
                Username = "user2",
                Password = "hashed_password",
                Status = "Active",
                Role = "Employee",
                CreatedAt = DateTime.Now
            };
            _context.Employees.AddRange(employee1, employee2);
            await _context.SaveChangesAsync();

            SetupAuthenticatedUser(employee1.EmployeeID, "user1", "Employee");

            // Act
            var result = await _controller.GetEmployee(employee2.EmployeeID);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact] // 3
        public async Task GetEmployee_ShouldReturnEmployee_WhenAdminRequests_AnyProfile() // +ve case when admin requests any profile
        {
            // Arrange
            var admin = new Employee
            {
                Name = "Admin User",
                Username = "admin",
                Password = "hashed_password",
                Status = "Active",
                Role = "Admin",
                CreatedAt = DateTime.Now
            };
            var employee = new Employee
            {
                Name = "Regular User",
                Username = "regularuser",
                Password = "hashed_password",
                Status = "Active",
                Role = "Employee",
                CreatedAt = DateTime.Now
            };
            _context.Employees.AddRange(admin, employee);
            await _context.SaveChangesAsync();

            SetupAuthenticatedUser(admin.EmployeeID, "admin", "Admin");

            // Act
            var result = await _controller.GetEmployee(employee.EmployeeID);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEmployee = Assert.IsType<Employee>(okResult.Value);
            Assert.Equal("regularuser", returnedEmployee.Username);
        }

        [Fact] // 4
        public async Task GetEmployee_ShouldReturnNotFound_WhenOwnProfileDoesNotExist() // -ve when own profile doesn't exist
        {
            // Arrange - User claims they are ID 999, but that ID doesn't exist in DB
            SetupAuthenticatedUser(999, "testuser", "Employee");

            // Act
            var result = await _controller.GetEmployee(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetEmployee_ShouldReturnNotFound_WhenAdminRequests_NonExistentEmployee() // -ve when admin tries to access a non-existent employee
        {
            // Arrange
            var admin = new Employee
            {
                Name = "Admin User",
                Username = "admin",
                Password = "hashed_password",
                Status = "Active",
                Role = "Admin",
                CreatedAt = DateTime.Now
            };
            _context.Employees.Add(admin);
            await _context.SaveChangesAsync();

            SetupAuthenticatedUser(admin.EmployeeID, "admin", "Admin");

            // Act - Admin tries to access non-existent employee
            var result = await _controller.GetEmployee(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        private void SetupAuthenticatedUser(int userId, string username, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
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
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}