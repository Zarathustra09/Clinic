using System;
using System.ComponentModel.DataAnnotations;

namespace Clinic.Models
{
    public class AppointmentDto
    {
        public int Id { get; set; }

        [Display(Name = "Patient")]
        [Required(ErrorMessage = "Patient selection is required")]
        public int UserId { get; set; }

        [Display(Name = "Doctor")]
        [Required(ErrorMessage = "Doctor selection is required")]
        public int DoctorId { get; set; }

        [Display(Name = "Branch")]
        [Required(ErrorMessage = "Branch selection is required")]
        public int BranchId { get; set; }

        [Display(Name = "Reason")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; }

        [Display(Name = "Time Slot")]
        [Required(ErrorMessage = "Time slot selection is required")]
        public int TimeSlotId { get; set; }

        // Display properties - NOT required for model binding, populated on server side
        public string UserFullName { get; set; }
        public string BranchName { get; set; }
        public string DoctorName { get; set; }
        public string FormattedTimeRange { get; set; }

        // Time properties derived from TimeSlot - NOT required for model binding
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // For FullCalendar - computed properties with null checks
        public string Title => string.IsNullOrEmpty(UserFullName) ? "Appointment" :
            $"{UserFullName}" + (string.IsNullOrEmpty(Reason) ? "" : $" - {Reason}");

        public string Start => StartTime != DateTime.MinValue ? StartTime.ToString("yyyy-MM-ddTHH:mm:ss") : "";

        public string End => EndTime != DateTime.MinValue ? EndTime.ToString("yyyy-MM-ddTHH:mm:ss") : "";

        public string Color { get; set; } = "#007bff";
        
        public bool IsApproved { get; set; } = false;
    }
}