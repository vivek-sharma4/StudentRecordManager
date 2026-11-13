
using Dapper;
using StudentRecordManager.Data;
using StudentRecordManager.Models;
using StudentRecordManager.Repositories.Interfaces;

namespace StudentRecordManager.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly IDbDataConnection _connectionData;

        public DepartmentRepository(IDbDataConnection connectionData)
        {
            _connectionData = connectionData;
        }

        public async Task<IEnumerable<Department>> GetAllActiveDepartmentsAsync()
        {
            const string query = @"
                SELECT DepartmentID, DepartmentName, IsActive, CreatedAt
                FROM dbo.Departments
                WHERE IsActive = 1
                ORDER BY DepartmentName";

            await using var connection = _connectionData.CreateConnection();

            return await connection.QueryAsync<Department>(query);
        }

        public async Task<Department?> GetDepartmentByIdAsync(int departmentId)
        {
            const string query = @"
                SELECT DepartmentID, DepartmentName, IsActive, CreatedAt
                FROM dbo.Departments
                WHERE DepartmentID = @DepartmentID";

            await using var connection = _connectionData.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<Department>(
                query,
                new { DepartmentID = departmentId });
        }

        public async Task<int> CreateDepartmentAsync(string departmentName)
        {
            const string query = @"
                INSERT INTO dbo.Departments (DepartmentName, IsActive, CreatedAt)
                VALUES (@DepartmentName, 1, GETUTCDATE());

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var connection = _connectionData.CreateConnection();

            return await connection.QuerySingleAsync<int>(
                query,
                new { DepartmentName = departmentName });
        }
    }
}
