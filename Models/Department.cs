
using System.ComponentModel.DataAnnotations;

namespace StudentRecordManager.Models
{
    public class Department
    {
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Department name is required.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Department name must be between 2 and 100 characters.")]
        public string DepartmentName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
