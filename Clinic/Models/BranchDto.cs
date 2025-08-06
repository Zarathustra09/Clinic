using System.ComponentModel.DataAnnotations;

namespace Clinic.Models
{
    public class BranchDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Branch name is required")]
        [StringLength(255, ErrorMessage = "Branch name cannot exceed 255 characters")]
        [Display(Name = "Branch Name")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Total Appointments")]
        public int AppointmentCount { get; set; }
    }
}