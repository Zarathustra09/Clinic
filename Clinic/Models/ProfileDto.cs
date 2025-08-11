using System.ComponentModel.DataAnnotations;

namespace Clinic.Models
{
    public class ProfileDto
    {
        public int Id { get; set; }

        [Display(Name = "School/Employee ID")]
        public string? SchoolID { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Middle Name")]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Program/Specialization")]
        public string? Program { get; set; }

        [StringLength(500)]
        [Display(Name = "Profile Image")]
        public string? ProfileImage { get; set; }

        public int Role { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();
        
        public string RoleName => Role switch
        {
            0 => "Student",
            1 => "Doctor",
            2 => "Campus Clinic Staff",
            _ => "Unknown"
        };
    }
}