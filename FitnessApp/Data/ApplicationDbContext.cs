using FitnessApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet for Training Sessions
        public DbSet<TrainingSession> TrainingSessions { get; set; }

        // DbSet for Reservations
        public DbSet<Reservation> Reservations { get; set; }
        
        //DbSet  for Trainer and Trainrequest
        public DbSet<PersonalTrainingRequest> PersonalTrainingRequests { get; set; }
        public DbSet<Trainer> Trainers { get; set; }


    }
}
