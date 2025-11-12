using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MindfulMe.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual List<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
        public virtual List<MeditationSession> MeditationSessions { get; set; } = new List<MeditationSession>();

        public virtual int GetMaxJournalEntries() => 100;
        
        // Helper method for password hashing (simplified)
        public static string HashPassword(string password)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password)); // In real app, use proper hashing
        }
    }
}