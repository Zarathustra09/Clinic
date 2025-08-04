using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinic.Models
{
    [Table("TimeSlot", Schema = "Clinic")]
    public class TimeSlot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; } // References User.Id (Role = 3)

        [ForeignKey(nameof(DoctorId))]
        public User Doctor { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        // Optional: Reference to Appointment if the slot is booked
        public int? AppointmentId { get; set; }

        [ForeignKey(nameof(AppointmentId))]
        public Appointment Appointment { get; set; }

        public bool IsAvailable => AppointmentId == null;
    }
}