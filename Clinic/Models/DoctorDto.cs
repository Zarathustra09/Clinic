using System.ComponentModel.DataAnnotations;

namespace Clinic.Models
{
    public class DoctorDto
    {
        public int Id { get; set; }

        [StringLength(255)]
        [Display(Name = "Employee ID")]
        public string? SchoolID { get; set; } // Auto-generated, not user input

        [Required]
        [StringLength(255)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [StringLength(255)]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [StringLength(255)]
        [Display(Name = "Specialization")]
        public string Program { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();
    }
}