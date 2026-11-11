using Dapper;
using StudentRecordManager.Data;
using StudentRecordManager.Models;
using StudentRecordManager.Repositories.Interfaces;

namespace StudentRecordManager.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AccountRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            const string query = @"
                SELECT UserID, Email, PasswordHash, IsAdmin, Role, CreatedAt
                FROM Users
                WHERE Email = @Email";

            await using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<User>(query, new { Email = email });
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            const string query = @"
                SELECT UserID, Email, PasswordHash, IsAdmin, Role, CreatedAt
                FROM Users
                WHERE UserID = @UserID";

            await using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<User>(query, new { UserID = userId });
        }

        public async Task<int> CreateUserAsync(User user)
        {
            const string query = @"
                INSERT INTO Users (Email, PasswordHash, IsAdmin, Role, CreatedAt)
                VALUES (@Email, @PasswordHash, @IsAdmin, @Role, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<int>(query, user);
        }

        public async Task<bool> UpdateUserAuthorizationAsync(int userId, bool isAdmin)
        {
            const string query = @"
                UPDATE Users
                SET IsAdmin = @IsAdmin
                WHERE UserID = @UserID";

            await using var connection = _connectionFactory.CreateConnection();
            var rows = await connection.ExecuteAsync(query, new { UserID = userId, IsAdmin = isAdmin });
            return rows > 0;
        }

        public async Task<IEnumerable<User>> GetPendingUsersAsync()
        {
            const string query = @"
                SELECT UserID, Email, PasswordHash, IsAdmin, Role, CreatedAt
                FROM Users
                WHERE IsAdmin = 0 AND Role != 'Admin'
                ORDER BY CreatedAt DESC";

            await using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<User>(query);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            const string query = @"
                SELECT UserID, Email, PasswordHash, IsAdmin, Role, CreatedAt
                FROM Users
                ORDER BY CreatedAt DESC";

            await using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<User>(query);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            const string query = @"DELETE FROM Users WHERE UserID = @UserID";
            await using var connection = _connectionFactory.CreateConnection();
            var rows = await connection.ExecuteAsync(query, new { UserID = userId });
            return rows > 0;
        }
    }
}