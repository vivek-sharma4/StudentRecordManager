using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentRecordManager.Models;
using StudentRecordManager.Services.Interfaces;

namespace StudentRecordManager.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IDepartmentService _departmentService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(IStudentService studentService, IDepartmentService departmentService, ILogger<StudentController> logger)
        {
            _studentService = studentService;
            _departmentService = departmentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? department)
        {
            try
            {
                IEnumerable<Student> students;

                if (!string.IsNullOrWhiteSpace(department))
                {
                    department = department.Trim();

                    if (department.Length < 3)
                    {
                        ModelState.AddModelError("Department", "Please enter at least 3 characters to search.");
                        ViewBag.Department = department;
                        students = await _studentService.GetAllStudentsAsync();
                    }
                    else
                    {
                        students = await _studentService.SearchByDepartmentAsync(department);
                        ViewBag.Department = department;
                    }
                }
                else
                {
                    students = await _studentService.GetAllStudentsAsync();
                }

                await LoadDepartmentsAsync();
                return View(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student records. Department: {Department}", department);
                TempData["Error"] = "Unable to retrieve student records.";
                await LoadDepartmentsAsync();
                return View(Enumerable.Empty<Student>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (!ModelState.IsValid)
            {
                await LoadDepartmentsAsync(student.Department);
                var students = await _studentService.GetAllStudentsAsync();
                ViewBag.OpenCreateModal = true;
                return View("Index", students);
            }

            try
            {
                await _studentService.CreateStudentAsync(student);
                TempData["Success"] = "Student created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(Student.RollNo), ex.Message);
                await LoadDepartmentsAsync(student.Department);
                var students = await _studentService.GetAllStudentsAsync();
                ViewBag.OpenCreateModal = true;

                return View("Index", students);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadDepartmentsAsync(student.Department);
                var students = await _studentService.GetAllStudentsAsync();
                ViewBag.OpenCreateModal = true;

                return View("Index", students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student.");

                TempData["Error"] = "An unexpected error occurred while creating the student.";

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id <= 0)
                return BadRequest();

            try
            {
                var existingStudent = await _studentService.GetStudentByIdAsync(id);

                if (existingStudent == null)
                    return NotFound();

                student.StudentID = existingStudent.StudentID;
                student.Department = existingStudent.Department;

                if (!ModelState.IsValid)
                {
                    await LoadDepartmentsAsync();
                    var students = await _studentService.GetAllStudentsAsync();
                    ViewBag.EditStudentId = id;

                    return View("Index", students);
                }

                bool updated = await _studentService.UpdateStudentAsync(student);

                if (!updated)
                    return NotFound();

                TempData["Success"] = "Student updated successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(Student.RollNo), ex.Message);
                await LoadDepartmentsAsync();
                var students = await _studentService.GetAllStudentsAsync();
                ViewBag.EditStudentId = id;

                return View("Index", students);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadDepartmentsAsync();
                var students = await _studentService.GetAllStudentsAsync();
                ViewBag.EditStudentId = id;

                return View("Index", students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student {StudentID}.", id);
                TempData["Error"] = "Unable to update student details.";

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest();

                bool deleted = await _studentService.DeleteStudentAsync(id);
                if (!deleted)
                    return NotFound();

                TempData["Success"] = "Student deleted successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student {StudentID}.", id);
                TempData["Error"] = "Unable to delete student.";

                return RedirectToAction(nameof(Index));
            }
        }

        private async Task LoadDepartmentsAsync(string? selectedDepartment = null)
        {
            var departments = await _departmentService.GetAllActiveDepartmentsAsync();

            if (!User.IsInRole("Admin"))
            {
                var userDepartment = User.FindFirst("Department")?.Value;

                departments = departments
                    .Where(d => d.DepartmentName.Equals(
                        userDepartment,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();
                selectedDepartment = userDepartment;
            }

            ViewBag.Departments = departments;
            ViewBag.SelectedDepartment = selectedDepartment;
        }
    }
}