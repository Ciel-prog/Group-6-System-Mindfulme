namespace MindfulMe.API.DTOs
{
    public class CreateJournalEntryRequest
    {
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int MoodScore { get; set; }
        public int StressLevel { get; set; }
        public double HoursSlept { get; set; }
    }
}