using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Clinic.Models
{
    public class Branches
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        public ICollection<Appointment> Appointments { get; set; }
    }
}