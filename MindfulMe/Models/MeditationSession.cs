using System.ComponentModel.DataAnnotations;

namespace MindfulMe.Models
{
    public class MeditationSession
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public DateTime SessionDate { get; set; } = DateTime.Now;
        
        [Range(1, 120)]
        public int DurationMinutes { get; set; }
        
        [StringLength(100)]
        public string MeditationType { get; set; } = "Breathing";
        
        [Range(1, 5)]
        public int QualityRating { get; set; } // 1-5 stars
        
        // Foreign key
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}