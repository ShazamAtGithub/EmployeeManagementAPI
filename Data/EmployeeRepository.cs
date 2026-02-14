using System.Data;
using Microsoft.Data.SqlClient;
using EmployeeManagementAPI.Models;

namespace EmployeeManagementAPI.Data
{
    public class EmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Fetches the user and hashed password
        public async Task<Employee?> GetEmployeeByUsername(string username)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetEmployeeByUsername", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", username);

                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Employee
                            {
                                EmployeeID = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Username = reader.GetString(reader.GetOrdinal("Username")),
                                Password = reader.GetString(reader.GetOrdinal("Password")), 
                                Role = reader.GetString(reader.GetOrdinal("Role")),
                                Status = reader.GetString(reader.GetOrdinal("Status"))
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<int> RegisterEmployee(Employee employee)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_RegisterEmployee", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", employee.Name);
                    cmd.Parameters.AddWithValue("@Designation", employee.Designation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", employee.Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Department", employee.Department ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JoiningDate", employee.JoiningDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Skillset", employee.Skillset ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Username", employee.Username);
                    // Receives the HASHED password from the Controller
                    cmd.Parameters.AddWithValue("@Password", employee.Password);
                    cmd.Parameters.AddWithValue("@CreatedBy", employee.CreatedBy ?? "Self");
                    SqlParameter imageParam = new SqlParameter("@ProfileImage", SqlDbType.VarBinary, -1);
                    imageParam.Value = employee.ProfileImage ?? (object)DBNull.Value;
                    cmd.Parameters.Add(imageParam);

                    await conn.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task<Employee?> GetEmployeeById(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetEmployeeById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeID", id);

                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapEmployeeFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        public async Task<List<Employee>> GetAllEmployees()
        {
            var employees = new List<Employee>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAllEmployees", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            employees.Add(MapEmployeeFromReader(reader));
                        }
                    }
                }
            }
            return employees;
        }

        public async Task<bool> UpdateEmployee(Employee employee)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateEmployee", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeID", employee.EmployeeID);
                    cmd.Parameters.AddWithValue("@Name", employee.Name);
                    cmd.Parameters.AddWithValue("@Designation", employee.Designation ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", employee.Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Department", employee.Department ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JoiningDate", employee.JoiningDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Skillset", employee.Skillset ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ModifiedBy", employee.ModifiedBy ?? "System");
                    await conn.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }
        public async Task<bool> UpdateProfileImage(int employeeId, byte[]? imageBytes, string? modifiedBy)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateProfileImage", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                    cmd.Parameters.AddWithValue("@ModifiedBy", string.IsNullOrWhiteSpace(modifiedBy) ? "System" : modifiedBy);

                    SqlParameter imageParam = new SqlParameter("@ProfileImage", SqlDbType.VarBinary, -1);
                    imageParam.Value = imageBytes ?? (object)DBNull.Value;
                    cmd.Parameters.Add(imageParam);

                    await conn.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<byte[]?> GetProfileImage(int employeeId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetProfileImage", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeID", employeeId);

                    await conn.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        return (byte[])result;
                    }
                }
            }
            return null;
        }
        public async Task<bool> UpdateEmployeeStatus(int employeeId, string status, string modifiedBy)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateEmployeeStatus", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EmployeeID", employeeId);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@ModifiedBy", string.IsNullOrWhiteSpace(modifiedBy) ? "System" : modifiedBy);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var rowsAffected = reader.GetInt32(0);
                            return rowsAffected > 0;
                        }
                    }
                    return false;
                }
            }
        }

        private Employee MapEmployeeFromReader(SqlDataReader reader)
        {
            var employee = new Employee
            {
                EmployeeID = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Designation = reader.IsDBNull(reader.GetOrdinal("Designation")) ? null : reader.GetString(reader.GetOrdinal("Designation")),
                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                Department = reader.IsDBNull(reader.GetOrdinal("Department")) ? null : reader.GetString(reader.GetOrdinal("Department")),
                JoiningDate = reader.IsDBNull(reader.GetOrdinal("JoiningDate")) ? null : reader.GetDateTime(reader.GetOrdinal("JoiningDate")),
                Skillset = reader.IsDBNull(reader.GetOrdinal("Skillset")) ? null : reader.GetString(reader.GetOrdinal("Skillset")),
                Username = reader.GetString(reader.GetOrdinal("Username")),
                Password = reader.GetString(reader.GetOrdinal("Password")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")),
                ModifiedBy = reader.IsDBNull(reader.GetOrdinal("ModifiedBy")) ? null : reader.GetString(reader.GetOrdinal("ModifiedBy")),
                CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                ModifiedAt = reader.IsDBNull(reader.GetOrdinal("ModifiedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("ModifiedAt"))
            };

            // Safely map the Profile Image ONLY if the query returned it
            if (HasColumn(reader, "ProfileImage") && !reader.IsDBNull(reader.GetOrdinal("ProfileImage")))
            {
                employee.ProfileImage = (byte[])reader["ProfileImage"];
            }

            return employee;
        }

        // Helper method to safely check if a column exists in the current result set
        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}