using System.Data;
using System.Data.SqlClient;
using Divar.Models;

namespace Divar.DAL
{
    public class AdoUserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public AdoUserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = "SELECT COUNT(1) FROM AspNetUsers WHERE Email = @Email";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);

            var count = (int)await command.ExecuteScalarAsync();
            return count > 0;
        }

        public async Task AddUserAsync(CustomUser user)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"
            INSERT INTO AspNetUsers 
            (Id, UserName, Email, PhoneNumber, FirstName, LastName, PasswordHash, AccessFailedCount, LockoutEnabled, TwoFactorEnabled, EmailConfirmed, PhoneNumberConfirmed, SecurityStamp)
            VALUES 
            (@Id, @UserName, @Email, @PhoneNumber, @FirstName, @LastName, @PasswordHash, @AccessFailedCount, @LockoutEnabled, @TwoFactorEnabled, @EmailConfirmed, @PhoneNumberConfirmed, @SecurityStamp)";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", user.Id);
            command.Parameters.AddWithValue("@UserName", user.UserName);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
            command.Parameters.AddWithValue("@FirstName", user.FirstName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LastName", user.LastName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@AccessFailedCount", 0); // مقدار پیش‌فرض
            command.Parameters.AddWithValue("@LockoutEnabled", false); // مقدار پیش‌فرض
            command.Parameters.AddWithValue("@TwoFactorEnabled", false); // مقدار پیش‌فرض
            command.Parameters.AddWithValue("@EmailConfirmed", false); // مقدار پیش‌فرض
            command.Parameters.AddWithValue("@PhoneNumberConfirmed", false); // مقدار پیش‌فرض
            command.Parameters.AddWithValue("@SecurityStamp", Guid.NewGuid().ToString()); // مقدار پیش‌فرض

            await command.ExecuteNonQueryAsync();
        }

        public async Task<CustomUser?> GetUserByEmailAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = "SELECT * FROM AspNetUsers WHERE Email = @Email";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", email);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new CustomUser
                {
                    Id = reader["Id"].ToString(),
                    UserName = reader["UserName"].ToString(),
                    Email = reader["Email"].ToString(),
                    PhoneNumber = reader["PhoneNumber"].ToString(),
                    FirstName = reader["FirstName"]?.ToString(),
                    LastName = reader["LastName"]?.ToString(),
                    PasswordHash = reader["PasswordHash"]?.ToString()
                };
            }

            return null;
        }
    }
}