using System.ComponentModel.DataAnnotations;

namespace Clinic.Models
{
    public class RegisterDto
    {
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

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Role")]
        public int Role { get; set; } // Values: 1 (Student), 2 (Doctor), 3 (Campus Clinic)

        [StringLength(255)]
        [Display(Name = "Program")]
        public string Program { get; set; }
    }
}