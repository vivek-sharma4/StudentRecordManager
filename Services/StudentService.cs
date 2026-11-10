using Microsoft.Extensions.Logging;
using StudentRecordManager.Models;
using StudentRecordManager.Repositories.Interfaces;
using StudentRecordManager.Services.Interfaces;

namespace StudentRecordManager.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ILogger<StudentService> _logger;

        public StudentService(IStudentRepository studentRepository, ILogger<StudentService> logger)
        {
            _studentRepository = studentRepository;
            _logger = logger;
        }

        // Get all students
        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            return await _studentRepository.GetAllStudentsAsync();
        }

        // Get student by ID
        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            if (studentId <= 0) throw new ArgumentException("Invalid student ID.");

            return await _studentRepository.GetStudentByIdAsync(studentId);
        }

        // Create student
        public async Task<int> CreateStudentAsync(Student student)
        {
            ValidateStudent(student);

            student.Name = student.Name.Trim();
            student.RollNo = student.RollNo.Trim();
            student.Department = student.Department.Trim();

            bool exists = await _studentRepository.RollNoExistsAsync(student.RollNo);

            if (exists)
            {
                throw new InvalidOperationException("This roll number already exists.");
            }

            try
            {
                int studentId = await _studentRepository.CreateStudentAsync(student);

                _logger.LogInformation("Student created successfully. StudentID: {StudentID}", studentId);

                return studentId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student with RollNo: {RollNo}", student.RollNo);

                throw;
            }
        }

        // Update student
        public async Task<bool> UpdateStudentAsync(Student student)
        {
            if (student.StudentID <= 0) throw new ArgumentException("Invalid student ID.");

            ValidateStudent(student);

            student.Name = student.Name.Trim();
            student.RollNo = student.RollNo.Trim();
            student.Department = student.Department.Trim();

            var existingStudent = await _studentRepository.GetStudentByIdAsync(student.StudentID);

            if (existingStudent == null)
                return false;

            bool exists = await _studentRepository.RollNoExistsAsync(student.RollNo, student.StudentID);

            if (exists)
            {
                throw new InvalidOperationException("This roll number is already assigned to another student.");
            }

            try
            {
                bool updated = await _studentRepository.UpdateStudentAsync(student);

                if (updated)
                {
                    _logger.LogInformation("Student updated successfully. StudentID: {StudentID}", student.StudentID);
                }

                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student. StudentID: {StudentID}", student.StudentID);

                throw;
            }
        }

        // Delete student
        public async Task<bool> DeleteStudentAsync(int studentId)
        {
            if (studentId <= 0)
                throw new ArgumentException("Invalid student ID.");

            try
            {
                bool deleted = await _studentRepository.DeleteStudentAsync(studentId);

                if (deleted)
                {
                    _logger.LogInformation("Student deleted successfully. StudentID: {StudentID}", studentId);
                }

                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student. StudentID: {StudentID}", studentId);

                throw;
            }
        }

        // Search students by department
        public async Task<IEnumerable<Student>> SearchByDepartmentAsync(
            string department)
        {
            if (string.IsNullOrWhiteSpace(department))
                throw new ArgumentException("Department is required.");

            return await _studentRepository.SearchByDepartmentAsync(department.Trim());
        }

        // Validate student details
        private static void ValidateStudent(Student student)
        {
            if (student == null)
                throw new ArgumentNullException(nameof(student));

            if (string.IsNullOrWhiteSpace(student.Name))
                throw new ArgumentException("Student name is required.");

            if (string.IsNullOrWhiteSpace(student.RollNo))
                throw new ArgumentException("Roll number is required.");

            if (string.IsNullOrWhiteSpace(student.Department))
                throw new ArgumentException("Department is required.");

            if (student.Marks < 0 || student.Marks > 100)
                throw new ArgumentException("Marks must be between 0 and 100.");

            if (student.Name.Trim().Length > 100)
                throw new ArgumentException("Student name cannot exceed 100 characters.");

            if (student.RollNo.Trim().Length > 50)
                throw new ArgumentException("Roll number cannot exceed 50 characters.");

            if (student.Department.Trim().Length > 100)
                throw new ArgumentException("Department cannot exceed 100 characters.");
        }
    }
}