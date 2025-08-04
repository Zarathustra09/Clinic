using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Clinic.Models
{
    [Table("User", Schema = "Clinic")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        public string SchoolID { get; set; }

        [Required]
        [StringLength(255)]
        public string FirstName { get; set; }

        [StringLength(255)]
        public string MiddleName { get; set; }

        [Required]
        [StringLength(255)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        public int Role { get; set; } // Values: 1 (Student), 2 (Doctor), 3 (Campus Clinic)

        [StringLength(255)]
        public string Program { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for related appointments
        public ICollection<Appointment> Appointments { get; set; }
    }
}