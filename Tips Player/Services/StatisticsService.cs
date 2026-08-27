using Microsoft.Extensions.Logging;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;
using Tips_Player.Infrastructure.Persistence;
using Tips_Player.Constants;

namespace Tips_Player.Services;

public class StatisticsService : IStatisticsService
{
    private const string SessionsFileName = "play_sessions.json";
    private readonly string _sessionsPath;
    private readonly ILogger<StatisticsService> _logger;
    private readonly IAppDatabase _database;
    private readonly IJsonFileStore _jsonStore;
    private List<PlaySession> _sessions = [];

    public ListeningStats Stats { get; private set; } = new();

    public StatisticsService(ILogger<StatisticsService> logger, IAppDatabase database, IJsonFileStore jsonStore)
    {
        _logger = logger;
        _database = database;
        _jsonStore = jsonStore;
        _sessionsPath = Path.Combine(FileSystem.AppDataDirectory, SessionsFileName);
        _logger.LogInformation("StatisticsService initialized");
    }

    public async Task LoadStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _sessions = (await _database.GetSessionsAsync(cancellationToken)).ToList();
            if (_sessions.Count == 0 && File.Exists(_sessionsPath))
            {
                _sessions = await _jsonStore.ReadAsync<List<PlaySession>>(
                    _sessionsPath, AppConstants.Validation.MaxPersistedFileBytes, cancellationToken) ?? [];
                await _database.ReplaceSessionsAsync(_sessions, cancellationToken);
                File.Move(_sessionsPath, _sessionsPath + ".migrated", overwrite: true);
            }

            await RecalculateStatsAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading statistics");
        }
    }

    public async Task SaveStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _database.ReplaceSessionsAsync(_sessions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving statistics");
        }
    }

    public async Task RecordPlaySessionAsync(PlaySession session, CancellationToken cancellationToken = default)
    {
        _sessions.Add(session);
        await _database.AddSessionAsync(session, cancellationToken);
        await RecalculateStatsAsync(cancellationToken);
    }

    public async Task<ListeningStats> GetStatsForPeriodAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var periodSessions = _sessions.Where(s => s.StartTime >= start && s.StartTime <= end).ToList();
        return await CalculateStatsFromSessionsAsync(periodSessions, cancellationToken);
    }

    public async Task<List<TrackStats>> GetTopTracksAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Task.FromResult(Stats.TopTracks.Take(count).ToList());
    }

    public async Task<List<ArtistStats>> GetTopArtistsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Task.FromResult(Stats.TopArtists.Take(count).ToList());
    }

    public async Task<List<AlbumStats>> GetTopAlbumsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Task.FromResult(Stats.TopAlbums.Take(count).ToList());
    }

    public async Task ResetStatsAsync(CancellationToken cancellationToken = default)
    {
        _sessions.Clear();
        Stats = new ListeningStats();
        await _database.ClearSessionsAsync(cancellationToken);
    }

    private async Task RecalculateStatsAsync(CancellationToken cancellationToken = default)
    {
        Stats = await CalculateStatsFromSessionsAsync(_sessions, cancellationToken);
    }

    private Task<ListeningStats> CalculateStatsFromSessionsAsync(List<PlaySession> sessions, CancellationToken cancellationToken = default)
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
