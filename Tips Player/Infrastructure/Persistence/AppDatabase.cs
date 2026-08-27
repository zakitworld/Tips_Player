using System.Text.Json;
using SQLite;
using Tips_Player.Models;

namespace Tips_Player.Infrastructure.Persistence;

public interface IAppDatabase
{
    Task<IReadOnlyList<MediaItem>> GetMediaAsync(CancellationToken cancellationToken = default);
    Task ReplaceMediaAsync(IReadOnlyCollection<MediaItem> items, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlaySession>> GetSessionsAsync(CancellationToken cancellationToken = default);
    Task AddSessionAsync(PlaySession session, CancellationToken cancellationToken = default);
    Task ReplaceSessionsAsync(IReadOnlyCollection<PlaySession> sessions, CancellationToken cancellationToken = default);
    Task ClearSessionsAsync(CancellationToken cancellationToken = default);
}

public sealed class AppDatabase : IAppDatabase
{
    private readonly SQLiteAsyncConnection _connection;
    private readonly SemaphoreSlim _initializationGate = new(1, 1);
    private bool _initialized;

    public AppDatabase()
    {
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "tips-player.db3");
        _connection = new SQLiteAsyncConnection(databasePath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex);
    }

    public async Task<IReadOnlyList<MediaItem>> GetMediaAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await InitializeAsync().ConfigureAwait(false);
        var rows = await _connection.Table<MediaRow>().ToListAsync().ConfigureAwait(false);
        return rows.Select(row => row.ToModel()).ToList();
    }

    public async Task ReplaceMediaAsync(IReadOnlyCollection<MediaItem> items, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await InitializeAsync().ConfigureAwait(false);
        var rows = items.Select(MediaRow.FromModel).ToList();
        await _connection.RunInTransactionAsync(connection =>
        {
            connection.DeleteAll<MediaRow>();
            connection.InsertAll(rows, runInTransaction: false);
        }).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<PlaySession>> GetSessionsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await InitializeAsync().ConfigureAwait(false);
        var rows = await _connection.Table<PlaySessionRow>().OrderBy(row => row.StartTimeUtc).ToListAsync().ConfigureAwait(false);
        return rows.Select(row => row.ToModel()).ToList();
    }

    public async Task AddSessionAsync(PlaySession session, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await InitializeAsync().ConfigureAwait(false);
        await _connection.InsertAsync(PlaySessionRow.FromModel(session)).ConfigureAwait(false);
    }

    public async Task ReplaceSessionsAsync(IReadOnlyCollection<PlaySession> sessions, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await InitializeAsync().ConfigureAwait(false);
        var rows = sessions.Select(PlaySessionRow.FromModel).ToList();
        await _connection.RunInTransactionAsync(connection =>
        {
            connection.DeleteAll<PlaySessionRow>();
            connection.InsertAll(rows, runInTransaction: false);
        }).ConfigureAwait(false);
    }

    public async Task ClearSessionsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await InitializeAsync().ConfigureAwait(false);
        await _connection.DeleteAllAsync<PlaySessionRow>().ConfigureAwait(false);
    }

    private async Task InitializeAsync()
    {
        if (_initialized) return;
        await _initializationGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_initialized) return;
            await _connection.ExecuteAsync("PRAGMA journal_mode=WAL;").ConfigureAwait(false);
            await _connection.ExecuteAsync("PRAGMA foreign_keys=ON;").ConfigureAwait(false);
            await _connection.CreateTableAsync<MediaRow>().ConfigureAwait(false);
            await _connection.CreateTableAsync<PlaySessionRow>().ConfigureAwait(false);
            _initialized = true;
        }
        finally
        {
            _initializationGate.Release();
        }
    }

    private sealed class MediaRow
    {
        [PrimaryKey, MaxLength(64)] public string Id { get; set; } = string.Empty;
        [MaxLength(255)] public string Title { get; set; } = string.Empty;
        [MaxLength(255)] public string Artist { get; set; } = string.Empty;
        [MaxLength(255)] public string Album { get; set; } = string.Empty;
        [MaxLength(255)] public string Genre { get; set; } = string.Empty;
        public int Year { get; set; }
        [MaxLength(2048)] public string FilePath { get; set; } = string.Empty;
        [MaxLength(2048)] public string FolderPath { get; set; } = string.Empty;
        [MaxLength(255)] public string FolderName { get; set; } = string.Empty;
        public long DurationTicks { get; set; }
        public int MediaType { get; set; }
        [MaxLength(2048)] public string? AlbumArtPath { get; set; }
        public long DateAddedUtcTicks { get; set; }
        public long? LastPlayedUtcTicks { get; set; }
        public int PlayCount { get; set; }
        public bool IsFavorite { get; set; }
        public bool HasLyrics { get; set; }
        public string? Lyrics { get; set; }
        public int Rating { get; set; }
        public string TagsJson { get; set; } = "[]";
        public long LastPositionTicks { get; set; }

        public static MediaRow FromModel(MediaItem item) => new()
        {
            Id = item.Id, Title = item.Title, Artist = item.Artist, Album = item.Album, Genre = item.Genre,
            Year = item.Year, FilePath = item.FilePath, FolderPath = item.FolderPath, FolderName = item.FolderName,
            DurationTicks = item.Duration.Ticks, MediaType = (int)item.MediaType, AlbumArtPath = item.AlbumArtPath,
            DateAddedUtcTicks = item.DateAdded.ToUniversalTime().Ticks,
            LastPlayedUtcTicks = item.LastPlayedDate?.ToUniversalTime().Ticks, PlayCount = item.PlayCount,
            IsFavorite = item.IsFavorite, HasLyrics = item.HasLyrics, Lyrics = item.Lyrics, Rating = item.Rating,
            TagsJson = JsonSerializer.Serialize(item.Tags), LastPositionTicks = item.LastPosition.Ticks
        };

        public MediaItem ToModel() => new()
        {
            Id = Id, Title = Title, Artist = Artist, Album = Album, Genre = Genre, Year = Year,
            FilePath = FilePath, FolderPath = FolderPath, FolderName = FolderName,
            Duration = TimeSpan.FromTicks(DurationTicks), MediaType = (Tips_Player.Models.MediaType)MediaType,
            AlbumArtPath = AlbumArtPath, DateAdded = new DateTime(DateAddedUtcTicks, DateTimeKind.Utc).ToLocalTime(),
            LastPlayedDate = LastPlayedUtcTicks is { } ticks ? new DateTime(ticks, DateTimeKind.Utc).ToLocalTime() : null,
            PlayCount = PlayCount, IsFavorite = IsFavorite, HasLyrics = HasLyrics, Lyrics = Lyrics, Rating = Rating,
            Tags = JsonSerializer.Deserialize<List<string>>(TagsJson) ?? [], LastPosition = TimeSpan.FromTicks(LastPositionTicks)
        };
    }

    private sealed class PlaySessionRow
    {
        [PrimaryKey, AutoIncrement] public long Id { get; set; }
        [Indexed, MaxLength(64)] public string MediaId { get; set; } = string.Empty;
        [MaxLength(255)] public string Title { get; set; } = string.Empty;
        [MaxLength(255)] public string Artist { get; set; } = string.Empty;
        [MaxLength(255)] public string Album { get; set; } = string.Empty;
        [Indexed] public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public bool Completed { get; set; }

        public static PlaySessionRow FromModel(PlaySession session) => new()
        {
            MediaId = session.MediaId, Title = session.Title, Artist = session.Artist, Album = session.Album,
            StartTimeUtc = session.StartTime.ToUniversalTime(), EndTimeUtc = session.EndTime.ToUniversalTime(),
            Completed = session.Completed
        };

        public PlaySession ToModel() => new()
        {
            MediaId = MediaId, Title = Title, Artist = Artist, Album = Album,
            StartTime = DateTime.SpecifyKind(StartTimeUtc, DateTimeKind.Utc).ToLocalTime(),
            EndTime = DateTime.SpecifyKind(EndTimeUtc, DateTimeKind.Utc).ToLocalTime(), Completed = Completed
        };
    }
}
