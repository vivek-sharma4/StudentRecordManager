
using StudentRecordManager.Models;

namespace StudentRecordManager.Repositories.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetAllActiveDepartmentsAsync();

        Task<Department?> GetDepartmentByIdAsync(int departmentId);

        Task<int> CreateDepartmentAsync(string departmentName);
    }
}
