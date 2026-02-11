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

        public async Task<LoginResponse?> Login(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_LoginEmployee", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);

                    await conn.OpenAsync();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new LoginResponse
                            {
                                EmployeeID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Username = reader.GetString(2),
                                Role = reader.GetString(3),
                                Status = reader.GetString(4)
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
                    cmd.Parameters.AddWithValue("@Password", employee.Password);
                    cmd.Parameters.AddWithValue("@CreatedBy", employee.CreatedBy ?? "Self");

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
                    cmd.Parameters.AddWithValue("@Status", employee.Status ?? "Active");  // ADD THIS LINE
                    cmd.Parameters.AddWithValue("@ModifiedBy", employee.ModifiedBy ?? "System");

                    await conn.OpenAsync();
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }


        private Employee MapEmployeeFromReader(SqlDataReader reader)
        {
            return new Employee
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
        }
    }
}