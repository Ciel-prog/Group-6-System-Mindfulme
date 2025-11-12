using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MindfulMe.API.Models
{
    public class MeditationSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime SessionDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(1, 120)]
        public int DurationMinutes { get; set; }

        [MaxLength(50)]
        public string MeditationType { get; set; } = "Breathing";

        [Range(1, 5)]
        public int QualityRating { get; set; }

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}