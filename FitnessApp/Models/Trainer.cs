using System.ComponentModel.DataAnnotations;

namespace FitnessApp.Models
{
    public class Trainer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Trainer name cannot exceed 100 characters.")]
        public string Name { get; set; }

        // Navigation property for related TrainingSessions
        public ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();
    }
}
