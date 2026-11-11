using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // Display all students and search by department

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

                        return View(students);
                    }

                    students = await _studentService.SearchByDepartmentAsync(department);

                    ViewBag.Department = department;
                }
                else
                {
                    students = await _studentService.GetAllStudentsAsync();
                }

                return View(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving student records. Department: {Department}",
                    department);

                TempData["Error"] = "Unable to retrieve student records.";

                return View(Enumerable.Empty<Student>());
            }
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDepartmentsAsync();

            return View();
        }

        // Save new student
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (!ModelState.IsValid)
            {
                await LoadDepartmentsAsync(student.Department);
                return View(student);
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
                return View(student);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student.");

                ModelState.AddModelError(string.Empty, "An unexpected error occurred while creating the student.");

                await LoadDepartmentsAsync(student.Department);
                return View(student);
            }
        }

        // Display edit form
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);

                if (student == null)
                    return NotFound();

                await LoadDepartmentsAsync(student.Department);

                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student {StudentID} for editing.", id);

                TempData["Error"] = "Unable to retrieve student details.";

                return RedirectToAction(nameof(Index));
            }
        }

        // Update student
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Student student)
        {
            if (id != student.StudentID)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await LoadDepartmentsAsync(student.Department);
                return View(student);
            }

            try
            {
                bool updated = await _studentService.UpdateStudentAsync(student);

                if (!updated)
                    return NotFound();

                TempData["Success"] = "Student updated successfully!";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(Student.RollNo), ex.Message);
                return View(student);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student {StudentID}.", id);

                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the student.");

                return View(student);
            }
        }

        // Display delete confirmation
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);

                if (student == null)
                    return NotFound();

                return View(student);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student {StudentID} for deletion.", id);

                TempData["Error"] = "Unable to retrieve student details.";

                return RedirectToAction(nameof(Index));
            }
        }

        // Delete student
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
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
            var departments = await _departmentService
                .GetAllActiveDepartmentsAsync();

            ViewBag.Departments = new SelectList(
                departments,
                "DepartmentName",
                "DepartmentName",
                selectedDepartment);
        }
    }
}