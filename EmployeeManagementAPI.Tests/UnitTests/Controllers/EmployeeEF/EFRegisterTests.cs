using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementAPI.Controllers;
using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Services;
using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Models;

namespace EmployeeManagementAPI.Tests.UnitTests.Controllers.EmployeeEF
{
    public class RegisterTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IPasswordHasher> _mockHasher;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly EmployeeEFController _controller;

        public RegisterTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _mockHasher = new Mock<IPasswordHasher>();
            _mockJwtService = new Mock<IJwtService>();
            _controller = new EmployeeEFController(_context, _mockHasher.Object, _mockJwtService.Object);
        }

        [Fact] // Indicates test method
        public async Task Register_ShouldReturnOk_WhenValidRequest() // Register +ve test case
        {
            // Arrange (initalize test data and mock behavior)
            _mockHasher.Setup(h => h.Hash(It.IsAny<string>()))
                       .Returns("hashed_password");

            var request = new RegisterEmployeeRequest
            {
                Name = "John Doe",
                Username = "johndoe",
                Password = "Password123",
                Designation = "Developer",
                Department = "IT"
            };

            // Act (call the method being tested)
            var result = await _controller.Register(request);

            // Assert (verify the results)
            var okResult = Assert.IsType<OkObjectResult>(result);
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Username == "johndoe");

            Assert.NotNull(employee);

            Assert.Equal("John Doe", employee.Name);
            Assert.Equal("Active", employee.Status);
            Assert.Equal("Employee", employee.Role);
            Assert.Equal("hashed_password", employee.Password);

            _mockHasher.Verify(h => h.Hash("Password123"), Times.Once);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenUsername_AlreadyExists() // Username already exists -ve test case
        {
            // Arrange
            var existingEmployee = new Employee
            {
                Name = "Existing User",
                Username = "johndoe",
                Password = "hashedpass",
                Status = "Active",
                Role = "Employee",
                CreatedAt = DateTime.UtcNow
            };
            _context.Employees.Add(existingEmployee);
            await _context.SaveChangesAsync();

            var request = new RegisterEmployeeRequest
            {
                Name = "John Doe",
                Username = "johndoe",
                Password = "Password123"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenInvalidBase64Image() // Invalid Base64 image -ve test case
        {
            // Arrange
            var request = new RegisterEmployeeRequest
            {
                Name = "John Doe",
                Username = "johndoe",
                Password = "Password123",
                Base64ProfileImage = "not-valid-base64!!!"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenImage_Exceeds2MB() // Image size exceeds 2 MB -ve test case
        {
            // Arrange
            var largeImageBytes = new byte[3 * 1024 * 1024]; // 3 MB
            var largeBase64 = Convert.ToBase64String(largeImageBytes);

            var request = new RegisterEmployeeRequest
            {
                Name = "John Doe",
                Username = "johndoe",
                Password = "Password123",
                Base64ProfileImage = largeBase64
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_ShouldSaveImageBytes_WhenValidBase64Provided() // Valid Base64 image +ve test case
        {
            // Arrange
            var imageBytes = new byte[] { 1, 2, 3, 4, 5 };
            var base64Image = Convert.ToBase64String(imageBytes);

            _mockHasher.Setup(h => h.Hash(It.IsAny<string>()))
                       .Returns("hashed_password");

            var request = new RegisterEmployeeRequest
            {
                Name = "John Doe",
                Username = "johndoe",
                Password = "Password123",
                Base64ProfileImage = base64Image
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Username == "johndoe");
            Assert.NotNull(employee);
            Assert.NotNull(employee!.ProfileImage);
            Assert.Equal(imageBytes, employee.ProfileImage);
        }
        [Fact]
        public async Task Register_ShouldReturnOk_WhenImage_IsExactly2MB() // Exactly 2 MB image +ve test case
        {
            // Arrange
            var exact2MBBytes = new byte[2 * 1024 * 1024]; // Exactly 2 MB
            var exact2MBBase64 = Convert.ToBase64String(exact2MBBytes);

            _mockHasher.Setup(h => h.Hash(It.IsAny<string>()))
                       .Returns("hashed_password");

            var request = new RegisterEmployeeRequest
            {
                Name = "Jane Doe",
                Username = "janedoe",
                Password = "Password123",
                Base64ProfileImage = exact2MBBase64
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Username == "janedoe");
            Assert.NotNull(employee);
            Assert.NotNull(employee!.ProfileImage);
            Assert.Equal(exact2MBBytes.Length, employee.ProfileImage.Length);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}