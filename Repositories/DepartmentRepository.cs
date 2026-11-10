
using System.Data;
using Dapper;
using StudentRecordManager.Data;
using StudentRecordManager.Models;
using StudentRecordManager.Repositories.Interfaces;

namespace StudentRecordManager.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DepartmentRepository(
            IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Department>>
            GetAllActiveDepartmentsAsync()
        {
            await using var connection =
                _connectionFactory.CreateConnection();

            return await connection.QueryAsync<Department>(
                "dbo.usp_Department_GetAll",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<Department?> GetDepartmentByIdAsync(
            int departmentId)
        {
            await using var connection =
                _connectionFactory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<Department>(
                "dbo.usp_Department_GetById",
                new { DepartmentID = departmentId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CreateDepartmentAsync(
            string departmentName)
        {
            await using var connection =
                _connectionFactory.CreateConnection();

            return await connection.QuerySingleAsync<int>(
                "dbo.usp_Department_Insert",
                new { DepartmentName = departmentName },
                commandType: CommandType.StoredProcedure);
        }
    }
}
