
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRecordManager.Models;
using StudentRecordManager.Services.Interfaces;

namespace StudentRecordManager.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<DepartmentController> _logger;

        public DepartmentController(IDepartmentService departmentService, ILogger<DepartmentController> logger)
        {
            _departmentService = departmentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var departments = await _departmentService.GetAllActiveDepartmentsAsync();

                return View(departments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve department list. Action: {Action}", nameof(Index));
                TempData["Error"] = "Unable to retrieve departments.";

                return View(Enumerable.Empty<Department>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string departmentName)
        {
            if (string.IsNullOrWhiteSpace(departmentName))
            {
                TempData["Error"] = "Department name is required.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _departmentService.CreateDepartmentAsync(departmentName);
                TempData["Success"] = "Department created successfully!";
            }
            catch (ArgumentException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create department. DepartmentName: {DepartmentName}", departmentName);
                TempData["Error"] = "Department could not be created. It may already exist.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
