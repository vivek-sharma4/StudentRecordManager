
using Dapper;
using StudentRecordManager.Data;
using StudentRecordManager.Models;
using StudentRecordManager.Repositories.Interfaces;

namespace StudentRecordManager.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly IDbDataConnection _connectionData;

        public StudentRepository(IDbDataConnection connectionData)
        {
            _connectionData = connectionData;
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            const string query = @"
                SELECT StudentID, Name, RollNo, Department, Marks
                FROM dbo.Students";

            await using var connection = _connectionData.CreateConnection();

            return await connection.QueryAsync<Student>(query);
        }

        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            const string query = @"
                SELECT StudentID, Name, RollNo, Department, Marks
                FROM dbo.Students
                WHERE StudentID = @StudentID";

            await using var connection = _connectionData.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<Student>(
                query,
                new { StudentID = studentId });
        }

        public async Task<int> CreateStudentAsync(Student student)
        {
            const string query = @"
                INSERT INTO dbo.Students (Name, RollNo, Department, Marks)
                VALUES (@Name, @RollNo, @Department, @Marks);

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var connection = _connectionData.CreateConnection();

            return await connection.QuerySingleAsync<int>(
                query,
                new
                {
                    student.Name,
                    student.RollNo,
                    student.Department,
                    student.Marks
                });
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            const string query = @"
                UPDATE dbo.Students
                SET Name = @Name,
                    RollNo = @RollNo,
                    Department = @Department,
                    Marks = @Marks
                WHERE StudentID = @StudentID";

            await using var connection = _connectionData.CreateConnection();

            var rowsAffected = await connection.ExecuteAsync(
                query,
                new
                {
                    student.StudentID,
                    student.Name,
                    student.RollNo,
                    student.Department,
                    student.Marks
                });

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteStudentAsync(int studentId)
        {
            const string query = @"
                DELETE FROM dbo.Students
                WHERE StudentID = @StudentID";

            await using var connection = _connectionData.CreateConnection();

            var rowsAffected = await connection.ExecuteAsync(
                query,
                new { StudentID = studentId });

            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Student>> SearchByDepartmentAsync(string department)
        {
            const string query = @"
                SELECT StudentID, Name, RollNo, Department, Marks
                FROM dbo.Students
                WHERE Department LIKE @Department";

            await using var connection = _connectionData.CreateConnection();

            return await connection.QueryAsync<Student>(
                query,
                new { Department = $"%{department}%" });
        }

        public async Task<bool> RollNoExistsAsync(string rollNo, int? excludeStudentId = null)
        {
            const string query = @"
                SELECT COUNT(1)
                FROM dbo.Students
                WHERE RollNo = @RollNo
                AND (@StudentID IS NULL OR StudentID <> @StudentID)";

            await using var connection = _connectionData.CreateConnection();

            var count = await connection.QuerySingleAsync<int>(
                query,
                new
                {
                    RollNo = rollNo,
                    StudentID = excludeStudentId
                });

            return count > 0;
        }
    }
}
