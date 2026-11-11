using Dapper;
using StudentRecordManager.Data;
using StudentRecordManager.Models;
using StudentRecordManager.Repositories.Interfaces;

namespace StudentRecordManager.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IDbDataConnection _connectionFactory;

        public AccountRepository(IDbDataConnection connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            const string query = @"
                SELECT UserID, Email, PasswordHash, IsAuthorised, Role, CreatedAt, Department
                FROM Users
                WHERE Email = @Email";

            await using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<User>(query, new { Email = email });
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            const string query = @"
                SELECT UserID, Email, PasswordHash, IsAuthorised, Role, CreatedAt, Department
                FROM Users
                WHERE UserID = @UserID";

            await using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<User>(query, new { UserID = userId });
        }

        public async Task<int> CreateUserAsync(User user)
        {
            const string query = @"
                INSERT INTO Users (Email, PasswordHash, IsAuthorised, Role, CreatedAt, Department)
                VALUES (@Email, @PasswordHash, @IsAuthorised, @Role, @CreatedAt, @Department);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var connection = _connectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<int>(query, user);
        }

        public async Task<bool> UpdateUserAuthorizationAsync(int userId, bool IsAuthorised)
        {
            const string query = @"
                UPDATE Users
                SET IsAuthorised = @IsAuthorised
                WHERE UserID = @UserID";

            await using var connection = _connectionFactory.CreateConnection();
            var rows = await connection.ExecuteAsync(query, new { UserID = userId, IsAuthorised = IsAuthorised });
            return rows > 0;
        }

        public async Task<IEnumerable<User>> GetPendingUsersAsync()
        {
            const string query = @"
                SELECT UserID, Email, PasswordHash, IsAuthorised, Role, CreatedAt, Department
                FROM Users
                WHERE IsAuthorised = 0 AND Role != 'Admin'
                ORDER BY CreatedAt DESC";

            await using var connection = _connectionFactory.CreateConnection();
            return await connection.QueryAsync<User>(query);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            const string query = @"
                SELECT UserID, Email, PasswordHash, IsAuthorised, Role, CreatedAt, Department
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

        public async Task<bool> ApproveUserAsync(int userId, string department)
        {
            const string query = @"
                UPDATE Users
                SET IsAuthorised = 1,
                Department = @Department
                WHERE UserID = @UserID";

            await using var connection = _connectionFactory.CreateConnection();

            var rows = await connection.ExecuteAsync(
                query,
                new
                {
                    UserID = userId,
                    Department = department
                });

            return rows > 0;
        }
    }
}