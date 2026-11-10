using StudentRecordManager.Models;

namespace StudentRecordManager.Services.Interfaces
{
    public interface IStudentService
    {
        Task<IEnumerable<Student>> GetAllStudentsAsync();

        Task<Student?> GetStudentByIdAsync(int studentId);

        Task<int> CreateStudentAsync(Student student);

        Task<bool> UpdateStudentAsync(Student student);

        Task<bool> DeleteStudentAsync(int studentId);
        Task<IEnumerable<Student>> SearchByDepartmentAsync(string department);
    }
}
