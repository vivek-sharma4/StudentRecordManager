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

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            const string query = @"
                SELECT UserID, Username, PasswordHash
                FROM Users
                WHERE Username = @Username";

            await using var connection = _connectionFactory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<User>(query, new { Username = username });
        }

        public async Task<int> CreateUserAsync(User user)
        {
            const string query = @"
                INSERT INTO Users (Username, PasswordHash)
                VALUES (@Username, @PasswordHash);

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var connection = _connectionFactory.CreateConnection();

            return await connection.QuerySingleAsync<int>(query, user);
        }
    }
}