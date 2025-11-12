using MindfulMe.Models;

namespace MindfulMe.Services
{
    public record StreakResult(int CurrentStreak, int BestStreak, DateTime? LastActivityDate);

    public class StreakService
    {
        private readonly DatabaseService _db;

        public StreakService(DatabaseService db)
        {
            _db = db;
        }

        // Public helpers
        public StreakResult GetJournalStreak(int userId)
        {
            var dates = _db.GetUserJournalEntryDates(userId)
                           .Select(d => DateOnly.FromDateTime(d))
                           .ToHashSet();

            var current = CalculateCurrentStreak(dates);
            var best = CalculateBestStreak(dates);
            var last = _db.GetUserJournalEntryDates(userId).FirstOrDefault();

            return new StreakResult(current, best, last == default ? null : (DateTime?)last);
        }

        public StreakResult GetMeditationStreak(int userId)
        {
            var dates = _db.GetUserMeditationSessionDates(userId)
                           .Select(d => DateOnly.FromDateTime(d))
                           .ToHashSet();

            var current = CalculateCurrentStreak(dates);
            var best = CalculateBestStreak(dates);
            var last = _db.GetUserMeditationSessionDates(userId).FirstOrDefault();

            return new StreakResult(current, best, last == default ? null : (DateTime?)last);
        }

        // Core algorithms (date-only)
        private static int CalculateCurrentStreak(HashSet<DateOnly> activityDates)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            int streak = 0;
            var day = today;

            // Count consecutive days backward from today
            while (activityDates.Contains(day))
            {
                streak++;
                day = day.AddDays(-1);
            }

            return streak;
        }

        private static int CalculateBestStreak(HashSet<DateOnly> activityDates)
        {
            if (!activityDates.Any())
                return 0;

            var ordered = activityDates.OrderBy(d => d).ToList();
            int best = 0;
            int currentRun = 1;

            for (int i = 1; i < ordered.Count; i++)
            {
                if (ordered[i].DayNumber == ordered[i - 1].DayNumber + 1)
                {
                    currentRun++;
                }
                else
                {
                    if (currentRun > best) best = currentRun;
                    currentRun = 1;
                }
            }

            if (currentRun > best) best = currentRun;
            return best;
        }
    }
}