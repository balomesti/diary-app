using diary_app.Models;
using System.Net.Http.Json;

namespace diary_app.Services;

public class CommunityStats
{
    public string DominantMood { get; set; } = "good";
    public int TotalEntriesToday { get; set; }
    public int ActiveUsersToday { get; set; }
    public double AvgCommunityScore { get; set; }
    public Dictionary<string, int> MoodCounts { get; set; } = new();
}

public class LeaderboardEntry
{
    public int UserId { get; set; }
    public string DisplayName { get; set; } = "";
    public string Initials { get; set; } = "";
    public string AvatarBg { get; set; } = "#EEEDFE";
    public string AvatarFg { get; set; } = "#534AB7";
    public bool IsCurrentUser { get; set; }
    public double AvgMoodScore { get; set; }
    public string CurrentMood { get; set; } = "good";
    public int Streak { get; set; }
}

public class CommunityService
{
    private readonly HttpClient _http;

    public CommunityService(HttpClient http)
    {
        _http = http;
    }

    public async Task<CommunityStats> GetCommunityStatsAsync(string period)
    {
        try
        {
            var response = await _http.GetAsync($"api/Community/stats?period={period}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CommunityApiStats>();
                if (result != null)
                {
                    return new CommunityStats
                    {
                        DominantMood = result.DominantMood,
                        TotalEntriesToday = result.TotalEntries,
                        ActiveUsersToday = result.ActiveUsers,
                        AvgCommunityScore = result.AvgCommunityScore,
                        MoodCounts = result.MoodCounts ?? new Dictionary<string, int>()
                    };
                }
            }
        }
        catch
        {
        }

        return new CommunityStats();
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(string period, string sortBy)
    {
        try
        {
            var response = await _http.GetAsync($"api/Community/leaderboard?period={period}&sortBy={sortBy}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<LeaderboardEntry>>();
                return result ?? new List<LeaderboardEntry>();
            }
        }
        catch
        {
        }

        return new List<LeaderboardEntry>();
    }

    private class CommunityApiStats
    {
        public string DominantMood { get; set; } = "good";
        public int TotalEntries { get; set; }
        public int ActiveUsers { get; set; }
        public double AvgCommunityScore { get; set; }
        public Dictionary<string, int>? MoodCounts { get; set; }
    }
}
