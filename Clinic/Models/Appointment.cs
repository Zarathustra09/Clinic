using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinic.Models
{
    [Table("Appointment", Schema = "Clinic")]
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public User Doctor { get; set; }

        [Required]
        public int BranchId { get; set; }

        [ForeignKey(nameof(BranchId))]
        public Branches Branch { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Required reference to TimeSlot for timing information
        [Required]
        public int TimeSlotId { get; set; }

        [ForeignKey(nameof(TimeSlotId))]
        public TimeSlot TimeSlot { get; set; }

        // Computed properties for display - now derived from TimeSlot and direct Doctor reference
        [NotMapped]
        public string UserFullName => User != null ? $"{User.FirstName} {User.LastName}" : "";

        [NotMapped]
        public string BranchName => Branch?.Name ?? "";

        [NotMapped]
        public string DoctorName => Doctor != null ? $"{Doctor.FirstName} {Doctor.LastName}" : "";

        [NotMapped]
        public DateTime StartTime => TimeSlot?.StartTime ?? DateTime.MinValue;

        [NotMapped]
        public DateTime EndTime => TimeSlot?.EndTime ?? DateTime.MinValue;

        [NotMapped]
        public TimeSpan Duration => TimeSlot != null ? TimeSlot.EndTime - TimeSlot.StartTime : TimeSpan.Zero;

        [NotMapped]
        public string FormattedTimeRange => TimeSlot != null ?
            $"{TimeSlot.StartTime:MMM d, yyyy h:mm tt} - {TimeSlot.EndTime:h:mm tt}" : "";
    }
}