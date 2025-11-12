using System.ComponentModel.DataAnnotations;

namespace MindfulMe.Models
{
    public class JournalEntry
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public DateTime EntryDate { get; set; } = DateTime.Now;
        
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
        
        [Range(1, 10)]
        public int MoodScore { get; set; } // 1-10 scale
        
        [Range(1, 10)]
        public int StressLevel { get; set; } // 1-10 scale
        
        [Range(0, 24)]
        public double HoursSlept { get; set; }
        
        // Foreign key and navigation property
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        
        public string GetMoodDescription()
        {
            return MoodScore switch
            {
                <= 3 => "Very Low",
                <= 5 => "Low",
                <= 7 => "Moderate",
                <= 9 => "Good",
                10 => "Excellent",
                _ => "Unknown"
            };
        }
    }
}