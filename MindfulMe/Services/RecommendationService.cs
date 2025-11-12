using MindfulMe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindfulMe.Services
{
    public class RecommendationService
    {
        // Rule-based system simulating logic programming
        private readonly List<Func<JournalEntry, string>> _rules;

        public RecommendationService()
        {
            _rules = new List<Func<JournalEntry, string>>
            {
                // Rule 1: High stress + low sleep -> Meditation
                entry => entry.StressLevel >= 8 && entry.HoursSlept < 6 ?
                    "Guided Meditation (High stress detected with poor sleep)" : string.Empty,

                // Rule 2: High stress -> Breathing exercise
                entry => entry.StressLevel >= 7 ?
                    "5-Minute Breathing Exercise (High stress level)" : string.Empty,

                // Rule 3: Low mood -> Journal prompts
                entry => entry.MoodScore <= 3 ?
                    "Reflective Journal Prompts (Low mood detected)" : string.Empty,

                // Rule 4: Poor sleep -> Sleep story
                entry => entry.HoursSlept < 6 ?
                    "Sleep Story Audio (Poor sleep detected)" : string.Empty,

                // Rule 5: Moderate stress -> Light exercise
                entry => entry.StressLevel >= 5 && entry.StressLevel < 7 ?
                    "Light Stretching Exercise (Moderate stress)" : string.Empty,

                // Rule 6: Good metrics -> Maintenance
                entry => entry.MoodScore >= 7 && entry.StressLevel <= 4 && entry.HoursSlept >= 7 ?
                    "Maintenance Meditation (You're doing great!)" : string.Empty,

                // Default rule
                entry => "Mindful Breathing (General wellness practice)"
            };
        }

        public string GetRecommendation(JournalEntry latestEntry)
        {
            // Find the first rule that fires (returns non-empty)
            foreach (var rule in _rules)
            {
                var recommendation = rule(latestEntry);
                if (!string.IsNullOrEmpty(recommendation))
                {
                    return recommendation;
                }
            }
            return "Take a mindful break today.";
        }

        // Multiple recommendations based on different conditions
        public List<string> GetMultipleRecommendations(JournalEntry latestEntry, List<JournalEntry> recentHistory)
        {
            var recommendations = new List<string>();

            // Check various conditions and add recommendations
            if (latestEntry.StressLevel >= 7)
                recommendations.Add("Stress Relief Bundle: Meditation + Breathing");

            if (recentHistory.Average(e => e.HoursSlept) < 6.5)
                recommendations.Add("Sleep Improvement Program");

            if (recentHistory.Average(e => e.MoodScore) < 5)
                recommendations.Add("Mood Boosting Activities");

            if (!recommendations.Any())
                recommendations.Add("Daily Wellness Routine");

            return recommendations;
        }

        // New: use the Prolog KB via the runner (async)
        public async Task<List<string>> GetRecommendationsFromPrologAsync(JournalEntry latestEntry)
        {
            if (latestEntry == null) return new List<string>();

            var prolog = new PrologService(); // default: relies on swipl on PATH
            int stress = latestEntry.StressLevel;
            int mood = latestEntry.MoodScore;
            int sleep = (int)Math.Round(latestEntry.HoursSlept);

            try
            {
                var recs = await prolog.GetRecommendationsAsync(stress, mood, sleep).ConfigureAwait(false);
                return recs.Distinct().ToList();
            }
            catch (Exception ex)
            {
                // return error as single-item list so caller can display fallback
                return new List<string> { $"Prolog error: {ex.Message}" };
            }
        }
    }
}