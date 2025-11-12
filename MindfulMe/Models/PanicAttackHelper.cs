using System;
using System.Collections.Generic;

namespace MindfulMe.Models
{
    public class PanicAttackHelper
    {
        private readonly string[] _groundingExercises = {
            "Name 5 things you can see",
            "Name 4 things you can touch",
            "Name 3 things you can hear",
            "Name 2 things you can smell",
            "Name 1 thing you can taste"
        };

        public string[] GetGroundingExercises() => (string[])_groundingExercises.Clone();

        public string GetGroundingStep(int index)
        {
            if (index < 0 || index >= _groundingExercises.Length) return string.Empty;
            return _groundingExercises[index];
        }

        // Returns the milliseconds for one full breath cycle based on breaths per minute.
        public int GetMillisPerBreath(int breathsPerMinute)
        {
            if (breathsPerMinute <= 0) breathsPerMinute = 6;
            return (int)Math.Round(60000.0 / breathsPerMinute);
        }

        // Simple helper that returns a short calming phrase sequence you can display.
        public IEnumerable<string> GetCalmingPhrases()
        {
            yield return "You are safe right now.";
            yield return "Focus on the present moment.";
            yield return "Try slow, steady breaths.";
            yield return "Name what you can see around you.";
            yield return "This will pass — one breath at a time.";
        }
    }
}