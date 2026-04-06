using diary_app.Models;

namespace diary_app.Services;

public class CommunityStats
{
    public string DominantMood       { get; set; } = "good";
    public int    TotalEntriesToday  { get; set; }
    public int    ActiveUsersToday   { get; set; }
    public double AvgCommunityScore  { get; set; }
    public Dictionary<string, int> MoodCounts { get; set; } = new();
}

public class LeaderboardEntry
{
    public string DisplayName    { get; set; } = "";
    public string Initials       { get; set; } = "";
    public string AvatarBg       { get; set; } = "#EEEDFE";
    public string AvatarFg       { get; set; } = "#534AB7";
    public bool   IsCurrentUser  { get; set; }
    public double AvgMoodScore   { get; set; }
    public string CurrentMood    { get; set; } = "good";
    public int    Streak         { get; set; }
}

public class CommunityService
{
    private readonly DiaryService _diaryService;

    private static readonly (string Bg, string Fg)[] AvatarColors =
    {
        ("#EEEDFE", "#534AB7"), ("#E1F5EE", "#0F6E56"), ("#FAECE7", "#993C1D"),
        ("#FBEAF0", "#993556"), ("#E6F1FB", "#185FA5"), ("#FAEEDA", "#854F0B"),
        ("#EAF3DE", "#3B6D11"), ("#F1EFE8", "#5F5E5A"),
    };

    private static readonly Dictionary<string, int> MoodScores = new()
    {
        ["terrible"] = 1, ["bad"] = 2, ["okay"] = 3, ["good"] = 4, ["amazing"] = 5
    };

    public CommunityService(DiaryService diaryService)
    {
        _diaryService = diaryService;
    }

    public async Task<CommunityStats> GetCommunityStatsAsync(string period)
    {
        var all     = await _diaryService.GetEntriesAsync();
        var entries = FilterByPeriod(all, period);

        var counts = entries
            .Where(e => !string.IsNullOrEmpty(e.Mood))
            .GroupBy(e => e.Mood!.ToLower())
            .ToDictionary(g => g.Key, g => g.Count());

        var dominant = counts.OrderByDescending(kv => kv.Value)
                             .Select(kv => kv.Key)
                             .FirstOrDefault("good");

        var avgScore = counts.Count > 0
            ? counts.Sum(kv => MoodScores.GetValueOrDefault(kv.Key, 3) * kv.Value)
              / (double)counts.Values.Sum()
            : 3.0;

        return new CommunityStats
        {
            DominantMood      = dominant,
            TotalEntriesToday = entries.Count,
            ActiveUsersToday  = 1,
            AvgCommunityScore = Math.Round(avgScore, 1),
            MoodCounts        = counts,
        };
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(string period, string sortBy)
    {
        var all     = await _diaryService.GetEntriesAsync();
        var entries = FilterByPeriod(all, period);

        var moodedEntries = entries.Where(e => !string.IsNullOrEmpty(e.Mood)).ToList();

        var avgScore = moodedEntries.Count > 0
            ? moodedEntries.Average(e => MoodScores.GetValueOrDefault(e.Mood!.ToLower(), 3))
            : 3.0;

        var streak   = CalculateStreak(all);
        var lastMood = moodedEntries.OrderByDescending(e => e.Date)
                                    .Select(e => e.Mood!.ToLower())
                                    .FirstOrDefault("good");

        var me = new LeaderboardEntry
        {
            DisplayName   = "You",
            Initials      = "ME",
            AvatarBg      = AvatarColors[0].Bg,
            AvatarFg      = AvatarColors[0].Fg,
            IsCurrentUser = true,
            AvgMoodScore  = Math.Round(avgScore, 1),
            CurrentMood   = lastMood,
            Streak        = streak,
        };

        var placeholders = new List<LeaderboardEntry>
        {
            new() { DisplayName = "Amara K.", Initials = "AK", AvatarBg = AvatarColors[1].Bg, AvatarFg = AvatarColors[1].Fg, AvgMoodScore = 4.8, CurrentMood = "amazing", Streak = 12 },
            new() { DisplayName = "Liam T.",  Initials = "LT", AvatarBg = AvatarColors[2].Bg, AvatarFg = AvatarColors[2].Fg, AvgMoodScore = 4.5, CurrentMood = "amazing", Streak = 8  },
            new() { DisplayName = "Priya M.", Initials = "PM", AvatarBg = AvatarColors[3].Bg, AvatarFg = AvatarColors[3].Fg, AvgMoodScore = 4.0, CurrentMood = "good",    Streak = 9  },
            new() { DisplayName = "Carlos R.",Initials = "CR", AvatarBg = AvatarColors[4].Bg, AvatarFg = AvatarColors[4].Fg, AvgMoodScore = 3.8, CurrentMood = "good",    Streak = 3  },
            new() { DisplayName = "Sophie L.",Initials = "SL", AvatarBg = AvatarColors[5].Bg, AvatarFg = AvatarColors[5].Fg, AvgMoodScore = 3.5, CurrentMood = "okay",    Streak = 6  },
        };

        var combined = placeholders.Append(me).ToList();

        return sortBy == "streak"
            ? combined.OrderByDescending(e => e.Streak).ToList()
            : combined.OrderByDescending(e => e.AvgMoodScore).ToList();
    }

    private static List<DiaryEntry> FilterByPeriod(IEnumerable<DiaryEntry> entries, string period)
    {
        var now = DateTime.Today;
        return period switch
        {
            "week"  => entries.Where(e => e.Date >= now.AddDays(-7)).ToList(),
            "month" => entries.Where(e => e.Date >= now.AddDays(-30)).ToList(),
            _       => entries.Where(e => e.Date.Date == now).ToList(),
        };
    }

    private static int CalculateStreak(IEnumerable<DiaryEntry> entries)
    {
        var dates = entries
            .Select(e => e.Date.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        if (dates.Count == 0) return 0;

        var streak  = 0;
        var current = DateTime.Today;

        foreach (var date in dates)
        {
            if (date == current || date == current.AddDays(-1))
            {
                streak++;
                current = date;
            }
            else break;
        }

        return streak;
    }
}