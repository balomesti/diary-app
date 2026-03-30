using System;

namespace diary_app.Models
{
    public class StreakInfo
    {
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime? LastEntryDate { get; set; }
        public bool HasEntryToday { get; set; }
    }
}