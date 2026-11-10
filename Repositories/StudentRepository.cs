using Dapper;
using StudentRecordManager.Data;
using StudentRecordManager.Models;
using StudentRecordManager.Repositories.Interfaces;

namespace StudentRecordManager.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public StudentRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        // Get all students
        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            const string query = @"
                SELECT StudentID, Name, RollNo, Department, Marks
                FROM Students
                ORDER BY StudentID DESC";

            await using var connection = _connectionFactory.CreateConnection();

            var students = await connection.QueryAsync<Student>(query);

            return students;
        }

        // Get student by ID
        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            const string query = @"
                SELECT StudentID, Name, RollNo, Department, Marks
                FROM Students
                WHERE StudentID = @StudentID";

            await using var connection = _connectionFactory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<Student>(query, new { StudentID = studentId });
        }

        // Create student
        public async Task<int> CreateStudentAsync(Student student)
        {
            const string query = @"
                INSERT INTO Students
                (Name, RollNo, Department, Marks)
                VALUES
                (@Name, @RollNo, @Department, @Marks);

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            await using var connection = _connectionFactory.CreateConnection();

            return await connection.QuerySingleAsync<int>(query, student);
        }

        // Update student
        public async Task<bool> UpdateStudentAsync(Student student)
        {
            const string query = @"
                UPDATE Students
                SET
                    Name = @Name,
                    RollNo = @RollNo,
                    Department = @Department,
                    Marks = @Marks
                WHERE StudentID = @StudentID";

            await using var connection = _connectionFactory.CreateConnection();

            var rowsAffected = await connection.ExecuteAsync(query, student);

            return rowsAffected > 0;
        }

        // Delete student
        public async Task<bool> DeleteStudentAsync(int studentId)
        {
            const string query = @"
                DELETE FROM Students
                WHERE StudentID = @StudentID";

            await using var connection = _connectionFactory.CreateConnection();

            var rowsAffected = await connection.ExecuteAsync(query, new { StudentID = studentId });

            return rowsAffected > 0;
        }

        // Search students by department
        public async Task<IEnumerable<Student>> SearchByDepartmentAsync(string department)
        {
            const string query = @"
                SELECT StudentID, Name, RollNo, Department, Marks
                FROM Students
                WHERE Department = @Department
                ORDER BY Name";

            await using var connection = _connectionFactory.CreateConnection();

            return await connection.QueryAsync<Student>(query, new { Department = department });
        }
        public async Task<bool> RollNoExistsAsync(string rollNo, int? excludeStudentId = null)
        {
            const string query = @"
                SELECT COUNT(1)
                FROM Students
                WHERE RollNo = @RollNo
                AND (@StudentID IS NULL OR StudentID <> @StudentID)";

            await using var connection = _connectionFactory.CreateConnection();

            var count = await connection.ExecuteScalarAsync<int>(query,new{RollNo = rollNo, StudentID = excludeStudentId});
            return count > 0;
        }
    }
}
