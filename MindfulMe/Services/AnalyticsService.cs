using MindfulMe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindfulMe.Services
{
    public class AnalyticsService
    {
        private readonly ApiService _apiService; // CHANGED from DatabaseService

        public AnalyticsService(ApiService apiService) // CHANGED parameter type
        {
            _apiService = apiService;
        }

        // CHANGED to async
        public async Task<double> CalculateAverageMoodAsync(int userId, Func<JournalEntry, bool> filter)
        {
            var allEntries = await _apiService.GetJournalEntriesAsync(userId); // CHANGED
            
            if (allEntries == null || !allEntries.Any())
                return 0;

            return allEntries
                .Where(filter)
                .Average(entry => entry.MoodScore);
        }

        // Pure function: no side effects, depends only on input
        public static string GetMoodTrend(double currentMood, double previousMood)
        {
            var difference = currentMood - previousMood;
            return difference switch
            {
                > 1 => "Significantly Improved",
                > 0.1 => "Slightly Improved",
                < -1 => "Significantly Worsened",
                < -0.1 => "Slightly Worsened",
                _ => "Stable"
            };
        }

        // CHANGED to async
        public async Task<List<string>> GetWeeklySummaryAsync(int userId)
        {
            var allEntries = await _apiService.GetJournalEntriesAsync(userId); // CHANGED
            
            if (allEntries == null)
                return new List<string>();

            var lastWeekEntries = allEntries
                .Where(e => e.EntryDate >= DateTime.Now.AddDays(-7))
                .ToList();
            
            return lastWeekEntries
                .GroupBy(entry => entry.EntryDate.DayOfWeek)
                .Select(group => 
                    $"{group.Key}: Avg Mood {group.Average(e => e.MoodScore):F1}, " +
                    $"Avg Sleep {group.Average(e => e.HoursSlept):F1}h")
                .ToList();
        }

        // CHANGED to async
        public async Task<(double avgMood, double avgSleep, int totalEntries)> GetOverallStatsAsync(int userId)
        {
            var entries = await _apiService.GetJournalEntriesAsync(userId); // CHANGED
            
            if (entries == null || !entries.Any())
                return (0, 0, 0);

            var result = entries.Aggregate(
                (moodSum: 0.0, sleepSum: 0.0, count: 0),
                (acc, entry) => (
                    acc.moodSum + entry.MoodScore,
                    acc.sleepSum + entry.HoursSlept,
                    acc.count + 1
                ));

            return (
                result.moodSum / result.count,
                result.sleepSum / result.count,
                result.count
            );
        }

        // CHANGED to async
        public async Task<List<(DateTime date, string moodDescription)>> GetMoodHistoryAsync(int userId)
        {
            var entries = await _apiService.GetJournalEntriesAsync(userId); // CHANGED
            
            if (entries == null)
                return new List<(DateTime, string)>();

            return entries
                .Select(entry => (entry.EntryDate, entry.GetMoodDescription()))
                .ToList();
        }
    }
}