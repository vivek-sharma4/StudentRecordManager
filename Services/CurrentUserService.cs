using StudentRecordManager.Services.Interfaces;

namespace StudentRecordManager.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public const string AdminDepartment = "Admin";

        private readonly IHttpContextAccessor _accessor;

        public CurrentUserService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        private System.Security.Claims.ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

        public string Department =>
            Principal?.FindFirst("Department")?.Value?.Trim() ?? string.Empty;

        public bool IsAdmin =>
            Principal?.IsInRole("Admin") == true ||
            string.Equals(Department, AdminDepartment, StringComparison.OrdinalIgnoreCase);
    }
}