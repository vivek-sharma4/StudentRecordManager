using Microsoft.Extensions.Logging;
using StudentRecordManager.Models;
using StudentRecordManager.Repositories.Interfaces;
using StudentRecordManager.Services.Interfaces;

namespace StudentRecordManager.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<StudentService> _logger;

        public StudentService(IStudentRepository studentRepository, ICurrentUserService currentUser, ILogger<StudentService> logger)
        {
            _studentRepository = studentRepository;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            var students = await _studentRepository.GetAllStudentsAsync();
            return ApplyDepartmentScope(students);
        }

        public async Task<Student?> GetStudentByIdAsync(int studentId)
        {
            if (studentId <= 0) throw new ArgumentException("Invalid student ID.");

            var student = await _studentRepository.GetStudentByIdAsync(studentId);

            if (student != null)
                EnsureCanAccess(student);

            return student;
        }

        // Create student
        public async Task<int> CreateStudentAsync(Student student)
        {
            if (student == null) throw new ArgumentNullException(nameof(student));

            ApplyDepartmentRule(student);
            ValidateStudent(student);

            student.Name = student.Name.Trim();
            student.RollNo = student.RollNo.Trim();
            student.Department = student.Department.Trim();

            bool exists = await _studentRepository.RollNoExistsAsync(student.RollNo);

            if (exists)
                throw new InvalidOperationException("This roll number already exists.");

            try
            {
                int studentId = await _studentRepository.CreateStudentAsync(student);

                _logger.LogInformation("Student created successfully. StudentID: {StudentID}, RollNo: {RollNo}, Department: {Department}, Marks: {Marks}", studentId, student.RollNo, student.Department, student.Marks);

                return studentId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student. RollNo: {RollNo}, Department: {Department}", student.RollNo, student.Department);
                throw;
            }
        }

        public async Task<bool> UpdateStudentAsync(Student student)
        {
            if (student == null) throw new ArgumentNullException(nameof(student));
            if (student.StudentID <= 0) throw new ArgumentException("Invalid student ID.");

            var existingStudent = await _studentRepository.GetStudentByIdAsync(student.StudentID);

            if (existingStudent == null)
                return false;

            EnsureCanAccess(existingStudent);

            ApplyDepartmentRule(student);
            ValidateStudent(student);

            student.Name = student.Name.Trim();
            student.RollNo = student.RollNo.Trim();
            student.Department = student.Department.Trim();

            bool exists = await _studentRepository.RollNoExistsAsync(student.RollNo, student.StudentID);

            if (exists)
                throw new InvalidOperationException("This roll number is already assigned to another student.");

            try
            {
                bool updated = await _studentRepository.UpdateStudentAsync(student);

                if (updated)
                {
                    _logger.LogInformation("Student updated successfully. StudentID: {StudentID}, RollNo: {RollNo}, Department: {Department}, Marks: {Marks}", student.StudentID, student.RollNo, student.Department, student.Marks);
                }

                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student. StudentID: {StudentID}, RollNo: {RollNo}", student.StudentID, student.RollNo);
                throw;
            }
        }

        public async Task<bool> DeleteStudentAsync(int studentId)
        {
            if (studentId <= 0)
                throw new ArgumentException("Invalid student ID.");

            var existingStudent = await _studentRepository.GetStudentByIdAsync(studentId);

            if (existingStudent == null)
                return false;

            EnsureCanAccess(existingStudent);

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

        public async Task<IEnumerable<Student>> SearchByDepartmentAsync(string department)
        {
            if (string.IsNullOrWhiteSpace(department))
                throw new ArgumentException("Department is required.");

            department = department.Trim();

            if (department.Length < 3)
                throw new ArgumentException("Please enter at least 3 characters to search.");

            var students = await _studentRepository.SearchByDepartmentAsync(department);
            return ApplyDepartmentScope(students);
        }


        private IEnumerable<Student> ApplyDepartmentScope(IEnumerable<Student> students)
        {
            if (_currentUser.IsAdmin)
                return students;

            return students.Where(s => IsOwnDepartment(s.Department)).ToList();
        }

        private bool IsOwnDepartment(string? department)
        {
            if (string.IsNullOrWhiteSpace(_currentUser.Department))
                return false;

            return string.Equals(department?.Trim(), _currentUser.Department, StringComparison.OrdinalIgnoreCase);
        }

        private void EnsureCanAccess(Student student)
        {
            if (_currentUser.IsAdmin || IsOwnDepartment(student.Department))
                return;

            _logger.LogWarning("Access denied. User department '{UserDepartment}' tried to access StudentID {StudentID} in department '{StudentDepartment}'.", _currentUser.Department, student.StudentID, student.Department);

            throw new UnauthorizedAccessException("You do not have permission to access this student record.");
        }

        private void ApplyDepartmentRule(Student student)
        {
            if (!_currentUser.IsAdmin)
                student.Department = _currentUser.Department;
        }
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