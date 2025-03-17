using System;
using System.ComponentModel.DataAnnotations;

namespace NiceAdmin.Models
{
    public class StudentModel
    {
        [Required]
        [Display(Name = "Id")]
        public int StudentID { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string StudentName { get; set; }

        [Required]
        public string EnrollmentNo { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; }

        [Required]
        [Display(Name ="RollNo")]
        public int RollNo { get; set; }

        [Required]
        public int CurrentSemester { get; set; }

        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string EmailInstitute { get; set; }

        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string EmailPersonal { get; set; }

        [Required]
        [Phone]
        [StringLength(15, ErrorMessage = "Contact Number cannot be longer than 15 characters.")]
        [DataType(DataType.PhoneNumber)]
        public string ContactNo { get; set; }

        public int? CastID { get; set; }

        public int? CityID { get; set; }

        public string? Remarks { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Age must be between 1 and 100.")]
        public int Age { get; set; }

        [Required(ErrorMessage = "Birth Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateTime BirthDate { get; set; }
    }
}
