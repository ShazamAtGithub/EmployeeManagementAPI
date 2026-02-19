using Xunit;
using EmployeeManagementAPI.Services;

namespace EmployeeManagementAPI.Tests.UnitTests.Services
{
    public class PasswordHasherTests
    {
        private readonly IPasswordHasher _hasher;

        public PasswordHasherTests()
        {
            _hasher = new PasswordHasher();
        }

        [Fact]
        public void Hash_ShouldReturn_NonEmptyString()
        {
            // Arrange
            string password = "MyPassword123";

            // Act
            string hash = _hasher.Hash(password);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
        }

        [Fact]
        public void Verify_ShouldReturn_True_WhenPasswordMatches()
        {
            // Arrange
            string password = "MyPassword123";
            string hash = _hasher.Hash(password);

            // Act
            bool result = _hasher.Verify(hash, password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Verify_ShouldReturn_False_WhenPassword_DoesNotMatch()
        {
            // Arrange
            string password = "MyPassword123";
            string wrongPassword = "WrongPassword";
            string hash = _hasher.Hash(password);

            // Act
            bool result = _hasher.Verify(hash, wrongPassword);

            // Assert
            Assert.False(result);
        }
    }
}