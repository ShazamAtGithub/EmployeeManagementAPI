using EmployeeManagementAPI.Controllers;
using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Models;
using EmployeeManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EmployeeManagementAPI.Tests.UnitTests.Controllers.EmployeeEF
{
    public class LoginTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IPasswordHasher> _mockHasher;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly EmployeeEFController _controller;
        private readonly SqliteConnection _connection;

        public LoginTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;
            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();
            _mockHasher = new Mock<IPasswordHasher>();
            _mockJwtService = new Mock<IJwtService>();
            _controller = new EmployeeEFController(_context, _mockHasher.Object, _mockJwtService.Object);
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WithToken_When_CredentialsAreValid() //  Login +ve test case
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Test User",
                Username = "testuser",
                Password = "hashed_password",
                Status = "Active",
                Role = "Employee",
                CreatedAt = DateTime.UtcNow
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            _mockHasher.Setup(h => h.Verify("hashed_password", "Password123"))
                       .Returns(true);

            _mockJwtService.Setup(j => j.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                           .Returns("test-jwt-token");

            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "Password123"
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResponse>(okResult.Value);

            Assert.Equal("test-jwt-token", response.Token);
            Assert.Equal("testuser", response.Username);
            Assert.Equal("Employee", response.Role);

            _mockHasher.Verify(h => h.Verify("hashed_password", "Password123"), Times.Once);
            _mockJwtService.Verify(j => j.GenerateToken(employee.EmployeeID, "testuser", "Employee"), Times.Once);
        }

        [Fact]
        public async Task Login_ShouldSucceed_WithDifferentUsernameCase() // +ve case for username case insensitivity
        {
            // Arrange - register as "johndoe"
            var employee = new Employee
            {
                Name = "John Doe",
                Username = "johndoe",
                Password = "hashed_password",
                Status = "Active",
                Role = "Employee",
                CreatedAt = DateTime.UtcNow
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            _mockHasher.Setup(h => h.Verify("hashed_password", "Password123"))
                       .Returns(true);
            _mockJwtService.Setup(j => j.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                           .Returns("token");

            var loginRequest = new LoginRequest
            {
                Username = "JOHNDOE",
                Password = "Password123"
            };

            // Act
            var result = await _controller.Login(loginRequest);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
        [Fact]
        public async Task Login_ShouldReturn_Unauthorized_When_UsernameDoesNotExist() // user name does not exist 
        {
            // Arrange
            var request = new LoginRequest
            {
                Username = "nonexistent",
                Password = "Password123"
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorizedResult.Value);

            _mockHasher.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockJwtService.Verify(j => j.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Login_ShouldReturn_Unauthorized_When_PasswordIsIncorrect() // password is incorrect
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Test User",
                Username = "testuser",
                Password = "hashed_password",
                Status = "Active",
                Role = "Employee",
                CreatedAt = DateTime.UtcNow
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            _mockHasher.Setup(h => h.Verify("hashed_password", "WrongPassword"))
                       .Returns(false);

            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "WrongPassword"
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
            _mockHasher.Verify(h => h.Verify("hashed_password", "WrongPassword"), Times.Once);
            _mockJwtService.Verify(j => j.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Login_ShouldReturn_Unauthorized_When_AccountIsInactive() // Inactive user login 
        {
            // Arrange
            var employee = new Employee
            {
                Name = "Inactive User",
                Username = "inactiveuser",
                Password = "hashed_password",
                Status = "Inactive",
                Role = "Employee",
                CreatedAt = DateTime.UtcNow
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            _mockHasher.Setup(h => h.Verify("hashed_password", "Password123"))
                       .Returns(true);

            var request = new LoginRequest
            {
                Username = "inactiveuser",
                Password = "Password123"
            };

            // Act
            var result = await _controller.Login(request);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
            _mockJwtService.Verify(j => j.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();

            _connection?.Close();
            _connection?.Dispose();
        }
    }
}