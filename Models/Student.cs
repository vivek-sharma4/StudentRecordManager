using System.ComponentModel.DataAnnotations;

namespace StudentRecordManager.Models
{
    public class Student
    {
        public int StudentID { get; set; }

        [Required(ErrorMessage = "Student name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Roll number is required.")]
        [StringLength(50)]
        public string RollNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "100", ErrorMessage = "Marks must be between 0 and 100.")]
        public decimal Marks { get; set; }
    }
}