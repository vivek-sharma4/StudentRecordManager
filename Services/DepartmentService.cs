
using Microsoft.Extensions.Logging;
using StudentRecordManager.Models;
using StudentRecordManager.Repositories.Interfaces;
using StudentRecordManager.Services.Interfaces;

namespace StudentRecordManager.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(IDepartmentRepository departmentRepository, ILogger<DepartmentService> logger)
        {
            _departmentRepository = departmentRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Department>>GetAllActiveDepartmentsAsync()
        {
            return await _departmentRepository.GetAllActiveDepartmentsAsync();
        }

        public async Task<Department?> GetDepartmentByIdAsync(int departmentId)
        {
            if (departmentId <= 0)
                throw new ArgumentException("Invalid department ID.");

            return await _departmentRepository.GetDepartmentByIdAsync(departmentId);
        }

        public async Task<int> CreateDepartmentAsync(string departmentName)
        {
            if (string.IsNullOrWhiteSpace(departmentName))
            {
                _logger.LogWarning("Department creation rejected. Reason: Department name is empty.");

                throw new ArgumentException("Department name is required.");
            }

            departmentName = departmentName.Trim();

            if (departmentName.Length < 2 || departmentName.Length > 100)
            {
                _logger.LogWarning("Department creation rejected. DepartmentNameLength: {DepartmentNameLength}", departmentName.Length);

                throw new ArgumentException("Department name must be between 2 and 100 characters.");
            }

            try
            {
                int departmentId = await _departmentRepository.CreateDepartmentAsync(departmentName);

                _logger.LogInformation("Department created successfully. DepartmentID: {DepartmentID}, DepartmentName: {DepartmentName}", departmentId, departmentName);

                return departmentId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department. DepartmentName: {DepartmentName}", departmentName);
                throw;
            }
        }
    }
}
