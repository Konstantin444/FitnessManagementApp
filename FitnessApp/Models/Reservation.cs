using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessApp.Models
{
    public class Reservation
    {
        [Key] // Primary key
        public int ReservationId { get; set; }

        [ForeignKey("User")] // Nullable foreign key to IdentityUser
        public string? UserId { get; set; } // ID of the member making the reservation
        public IdentityUser? User { get; set; } // Navigation property for the member

        [ForeignKey("TrainingSession")] // Nullable foreign key to TrainingSession
        public int? TrainingSessionId { get; set; } // ID of the reserved training session
        public TrainingSession? TrainingSession { get; set; } // Navigation property for the session

        [Required] // Date and time the reservation was made
        [Display(Name = "Reservation Time and Date")]
        public DateTime ReservationDateTime { get; set; } = DateTime.Now;
    }
}
