using Microsoft.AspNetCore.Identity;
using NuGet.DependencyResolver;

namespace FitnessApp.Models
{
    public class PersonalTrainingRequest
    {
        public int Id { get; set; }
        public string MemberId { get; set; }
        public int TrainerId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected

        public virtual IdentityUser Member { get; set; }
        public virtual Trainer Trainer { get; set; }
    }

}
