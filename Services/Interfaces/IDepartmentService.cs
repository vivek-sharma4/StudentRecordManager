
using StudentRecordManager.Models;

namespace StudentRecordManager.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IEnumerable<Department>> GetAllActiveDepartmentsAsync();

        Task<Department?> GetDepartmentByIdAsync(int departmentId);

        Task<int> CreateDepartmentAsync(string departmentName);
    }
}
