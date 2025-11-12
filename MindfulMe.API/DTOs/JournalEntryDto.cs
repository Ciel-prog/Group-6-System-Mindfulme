namespace MindfulMe.API.DTOs
{
    public class JournalEntryDto
    {
        public int Id { get; set; }
        public DateTime EntryDate { get; set; }
        public string Content { get; set; } = string.Empty;
        public int MoodScore { get; set; }
        public int StressLevel { get; set; }
        public double HoursSlept { get; set; }
        public int UserId { get; set; }
    }

 
}