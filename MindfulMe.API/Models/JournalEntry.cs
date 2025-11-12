using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MindfulMe.API.Models
{
    public class JournalEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Range(1, 10)]
        public int MoodScore { get; set; }

        [Range(1, 10)]
        public int StressLevel { get; set; }

        [Range(0, 24)]
        public double HoursSlept { get; set; }

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        // Helper method for mood description
        public string GetMoodDescription()
        {
            return MoodScore switch
            {
                >= 8 => "Great",
                >= 6 => "Good",
                >= 4 => "Okay",
                >= 2 => "Low",
                _ => "Very Low"
            };
        }
    }
}