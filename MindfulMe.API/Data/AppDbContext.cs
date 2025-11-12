using Microsoft.EntityFrameworkCore;
using MindfulMe.API.Models;

namespace MindfulMe.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<MeditationSession> MeditationSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed initial data with STATIC values (no runtime hashing/salts)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "demo",
                    Email = "demo@mindfulme.com",
                    // Static bcrypt hash for the password "demo"
                    PasswordHash = "$2a$11$/kQfKaTPV31VpB82SIRW3OtQ/FapTbI/5GwM5gssB0sk9BnD4kkHO",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}