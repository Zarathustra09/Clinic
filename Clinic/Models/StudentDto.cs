using System.ComponentModel.DataAnnotations;

namespace Clinic.Models
{
    public class StudentDto
    {
        public int Id { get; set; }

        [StringLength(255)]
        [Display(Name = "School ID")]
        public string SchoolID { get; set; }

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
        [Display(Name = "Program")]
        public string Program { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Full name property for display
        public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();
    }
}