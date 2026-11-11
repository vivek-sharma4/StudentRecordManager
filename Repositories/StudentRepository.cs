using System.Data;
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

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            await using var connection = _connectionFactory.CreateConnection();

            return await connection.QueryAsync<Student>(
                "dbo.usp_Student_GetAll", commandType: CommandType.StoredProcedure);
        }

        // Get student by ID
        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            await using var connection = _connectionFactory.CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<Student>(
                "dbo.usp_Student_GetById",
                new { StudentID = studentId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> CreateStudentAsync(Student student)
        {
            await using var connection = _connectionFactory.CreateConnection();

            return await connection.QuerySingleAsync<int>(
                "dbo.usp_Student_Insert",
                new
                {
                    student.Name,
                    student.RollNo,
                    student.Department,
                    student.Marks
                },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var rowsAffected =
                await connection.QuerySingleAsync<int>(
                    "dbo.usp_Student_Update",
                    new
                    {
                        student.StudentID,
                        student.Name,
                        student.RollNo,
                        student.Department,
                        student.Marks
                    },
                    commandType: CommandType.StoredProcedure);

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteStudentAsync(int studentId)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var rowsAffected =
                await connection.QuerySingleAsync<int>(
                    "dbo.usp_Student_Delete",
                    new
                    {
                        StudentID = studentId
                    },
                    commandType: CommandType.StoredProcedure);

            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Student>> SearchByDepartmentAsync(
            string department)
        {
            await using var connection = _connectionFactory.CreateConnection();

            return await connection.QueryAsync<Student>(
                "dbo.usp_Student_SearchByDepartment",
                new { Department = department },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> RollNoExistsAsync(
            string rollNo,
            int? excludeStudentId = null)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var count =
                await connection.QuerySingleAsync<int>(
                    "dbo.usp_Student_RollNoExists",
                    new
                    {
                        RollNo = rollNo,
                        StudentID = excludeStudentId
                    },
                    commandType: CommandType.StoredProcedure);

            return count > 0;
        }
    }
}