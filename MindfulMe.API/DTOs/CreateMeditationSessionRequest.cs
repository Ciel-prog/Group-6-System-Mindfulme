
namespace MindfulMe.API.DTOs
{
    public class CreateMeditationSessionRequest
    {
        public int UserId { get; set; }
        public int DurationMinutes { get; set; }
        public string? MeditationType { get; set; }
        public int QualityRating { get; set; }
    }
}