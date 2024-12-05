using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessApp.Models
{
    public class TrainingSession
    {
        [Key] // Primary key
        public int SessionId { get; set; }

        [Required(ErrorMessage = "Session name is required.")]
        [StringLength(100, ErrorMessage = "The session name cannot exceed 100 characters.")]
        public string Name { get; set; } // Name of the session (e.g., "Yoga")

        public int? TrainerId { get; set; }

        [ForeignKey("TrainerId")]
        public Trainer? Trainer { get; set; } // Name of the trainer conducting the session

        [Required(ErrorMessage = "Date and time are required.")]
        [Display(Name = "Session Date and Time")]
        public DateTime SessionDateTime { get; set; } // Date and time of the session

        [Required(ErrorMessage = "Maximum participants is required.")]
        [Range(1, 100, ErrorMessage = "Maximum participants must be between 1 and 100.")]
        public int MaxParticipants { get; set; } // Maximum number of participants allowed in the session

        // Navigation property for reservations
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        // Computed property for available slots
        [NotMapped]
        public int AvailableSlots => MaxParticipants - Reservations.Count;
    }

}
