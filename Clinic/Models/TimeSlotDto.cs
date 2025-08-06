using System;
using System.ComponentModel.DataAnnotations;

namespace Clinic.Models
{
    public class TimeSlotDto
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        [Display(Name = "Appointment")]
        public int? AppointmentId { get; set; }

        // Navigation properties for display - made nullable to prevent validation errors
        public string? DoctorName { get; set; }
        public string? AppointmentReason { get; set; }
        public bool IsAvailable => AppointmentId == null;

        // Helper properties for form display - not required for API operations
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public TimeOnly StartTimeOnly { get; set; }

        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public TimeOnly EndTimeOnly { get; set; }

        // Additional computed properties for better UI display
        public string TimeRange => $"{StartTime:HH:mm} - {EndTime:HH:mm}";
        public string DateString => StartTime.ToString("MMM dd, yyyy");
        public string StatusText => IsAvailable ? "Available" : "Booked";
        public string StatusBadgeClass => IsAvailable ? "bg-success" : "bg-warning";
    }
}