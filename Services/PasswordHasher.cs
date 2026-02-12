using BCrypt.Net;

namespace EmployeeManagementAPI.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            // Generates a salt and hashes the password
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string passwordHash, string inputPassword)
        {
            // Verifies the password against the hash
            return BCrypt.Net.BCrypt.Verify(inputPassword, passwordHash);
        }
    }
}