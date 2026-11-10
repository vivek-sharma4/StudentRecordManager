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
        private readonly ILogger<StudentController> _logger;

        public StudentController(IStudentService studentService, ILogger<StudentController> logger)
        {
            _studentService = studentService;
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
                _logger.LogError(ex, "Error retrieving student records.");

                TempData["Error"] = "Unable to retrieve student records.";

                return View(Enumerable.Empty<Student>());
            }
        }

        // Display create form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Save new student
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Student student)
        {
            if (!ModelState.IsValid)
                return View(student);

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
                return View(student);

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
    }
}