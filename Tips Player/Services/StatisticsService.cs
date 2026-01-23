using System.Text.Json;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.Services;

public class StatisticsService : IStatisticsService
{
    private const string StatsFileName = "listening_stats.json";
    private const string SessionsFileName = "play_sessions.json";
    private readonly string _statsPath;
    private readonly string _sessionsPath;
    private List<PlaySession> _sessions = [];

    public ListeningStats Stats { get; private set; } = new();

    public StatisticsService()
    {
        _statsPath = Path.Combine(FileSystem.AppDataDirectory, StatsFileName);
        _sessionsPath = Path.Combine(FileSystem.AppDataDirectory, SessionsFileName);
    }

    public async Task LoadStatsAsync()
    {
        try
        {
            if (File.Exists(_sessionsPath))
            {
                var json = await File.ReadAllTextAsync(_sessionsPath);
                _sessions = JsonSerializer.Deserialize<List<PlaySession>>(json) ?? [];
            }

            await RecalculateStatsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading stats: {ex.Message}");
        }
    }

    public async Task SaveStatsAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_sessions);
            await File.WriteAllTextAsync(_sessionsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving stats: {ex.Message}");
        }
    }

    public async Task RecordPlaySessionAsync(PlaySession session)
    {
        _sessions.Add(session);
        await SaveStatsAsync();
        await RecalculateStatsAsync();
    }

    public async Task<ListeningStats> GetStatsForPeriodAsync(DateTime start, DateTime end)
    {
        var periodSessions = _sessions.Where(s => s.StartTime >= start && s.StartTime <= end).ToList();
        return await CalculateStatsFromSessionsAsync(periodSessions);
    }

    public async Task<List<TrackStats>> GetTopTracksAsync(int count = 10)
    {
        return await Task.FromResult(Stats.TopTracks.Take(count).ToList());
    }

    public async Task<List<ArtistStats>> GetTopArtistsAsync(int count = 10)
    {
        return await Task.FromResult(Stats.TopArtists.Take(count).ToList());
    }

    public async Task<List<AlbumStats>> GetTopAlbumsAsync(int count = 10)
    {
        return await Task.FromResult(Stats.TopAlbums.Take(count).ToList());
    }

    public async Task ResetStatsAsync()
    {
        _sessions.Clear();
        Stats = new ListeningStats();
        await SaveStatsAsync();
    }

    private async Task RecalculateStatsAsync()
    {
        Stats = await CalculateStatsFromSessionsAsync(_sessions);
    }

    private Task<ListeningStats> CalculateStatsFromSessionsAsync(List<PlaySession> sessions)
    {
        var stats = new ListeningStats();

        if (!sessions.Any())
        {
            return Task.FromResult(stats);
        }

        stats.TotalTracksPlayed = sessions.Count;
        stats.TotalListeningTime = TimeSpan.FromTicks(sessions.Sum(s => s.Duration.Ticks));
        stats.FirstListenDate = sessions.Min(s => s.StartTime);
        stats.LastListenDate = sessions.Max(s => s.StartTime);
        stats.TotalSessions = sessions.Select(s => s.StartTime.Date).Distinct().Count();

        // Calculate top tracks
        var trackGroups = sessions
            .GroupBy(s => new { s.Title, s.Artist })
            .Select(g => new TrackStats
            {
                TrackTitle = g.Key.Title,
                ArtistName = g.Key.Artist,
                PlayCount = g.Count(),
                TotalListenTime = TimeSpan.FromTicks(g.Sum(s => s.Duration.Ticks))
            })
            .OrderByDescending(t => t.PlayCount)
            .Take(50)
            .ToList();

        for (int i = 0; i < trackGroups.Count; i++)
        {
            trackGroups[i].Rank = i + 1;
        }
        stats.TopTracks = trackGroups;

        // Calculate top artists
        var artistGroups = sessions
            .GroupBy(s => s.Artist)
            .Select(g => new ArtistStats
            {
                ArtistName = g.Key,
                PlayCount = g.Count(),
                TotalListenTime = TimeSpan.FromTicks(g.Sum(s => s.Duration.Ticks))
            })
            .OrderByDescending(a => a.PlayCount)
            .Take(50)
            .ToList();

        for (int i = 0; i < artistGroups.Count; i++)
        {
            artistGroups[i].Rank = i + 1;
        }
        stats.TopArtists = artistGroups;

        // Calculate top albums
        var albumGroups = sessions
            .Where(s => !string.IsNullOrEmpty(s.Album))
            .GroupBy(s => new { s.Album, s.Artist })
            .Select(g => new AlbumStats
            {
                AlbumName = g.Key.Album,
                ArtistName = g.Key.Artist,
                PlayCount = g.Count(),
                TotalListenTime = TimeSpan.FromTicks(g.Sum(s => s.Duration.Ticks))
            })
            .OrderByDescending(a => a.PlayCount)
            .Take(50)
            .ToList();

        for (int i = 0; i < albumGroups.Count; i++)
        {
            albumGroups[i].Rank = i + 1;
        }
        stats.TopAlbums = albumGroups;

        // Calculate hourly listening pattern
        stats.HourlyListening = sessions
            .GroupBy(s => s.StartTime.Hour)
            .ToDictionary(g => g.Key, g => g.Count());

        // Calculate daily listening pattern
        stats.DailyListening = sessions
            .GroupBy(s => s.StartTime.DayOfWeek)
            .ToDictionary(g => g.Key, g => TimeSpan.FromTicks(g.Sum(s => s.Duration.Ticks)));

        return Task.FromResult(stats);
    }
}
