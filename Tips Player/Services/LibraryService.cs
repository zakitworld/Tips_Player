using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;
using Tips_Player.Constants;
using Tips_Player.Infrastructure.Validation;
using Tips_Player.Infrastructure.Persistence;

namespace Tips_Player.Services;

public class LibraryService : ILibraryService, IDisposable
{
    private const string LibraryFileName = "library.json";
    private readonly string _libraryPath;
    private readonly ILogger<LibraryService> _logger;
    private readonly IMediaScannerService _scanner;
    private readonly IAppDatabase _database;
    private readonly IJsonFileStore _jsonStore;
    private readonly IAlbumArtService _albumArtService;
    private CancellationTokenSource? _scanCancellation;
    private Task _deviceScanTask = Task.CompletedTask;

    public Task DeviceScanTask => _deviceScanTask;

    public ObservableCollection<MediaItem> MediaItems { get; } = [];
    public ObservableCollection<MediaItem> Songs { get; } = [];
    public ObservableCollection<MediaItem> Videos { get; } = [];
    public ObservableCollection<Artist> Artists { get; } = [];
    public ObservableCollection<Album> Albums { get; } = [];
    public ObservableCollection<Folder> Folders { get; } = [];
    public ObservableCollection<SmartPlaylist> SmartPlaylists { get; } = [];

    public LibraryService(
        ILogger<LibraryService> logger,
        IMediaScannerService scanner,
        IAppDatabase database,
        IJsonFileStore jsonStore,
        IAlbumArtService albumArtService)
    {
        _logger = logger;
        _scanner = scanner;
        _database = database;
        _jsonStore = jsonStore;
        _albumArtService = albumArtService;
        _libraryPath = Path.Combine(FileSystem.AppDataDirectory, LibraryFileName);
        _logger.LogInformation("LibraryService initialized");
        InitializeSmartPlaylists();
    }

    private void InitializeSmartPlaylists()
    {
        SmartPlaylists.Add(SmartPlaylist.CreateLikedSongs());
        SmartPlaylists.Add(SmartPlaylist.CreateRecentlyPlayed());
        SmartPlaylists.Add(SmartPlaylist.CreateMostPlayed());
        SmartPlaylists.Add(SmartPlaylist.CreateWithLyrics());
    }

    public async Task LoadLibraryAsync(CancellationToken cancellationToken = default)
    {
        // 1. Load persisted library (fast)
        try
        {
            var items = (await _database.GetMediaAsync(cancellationToken)).ToList();

            // One-time migration from the legacy bounded JSON store.
            if (items.Count == 0 && File.Exists(_libraryPath))
            {
                items = await _jsonStore.ReadAsync<List<MediaItem>>(
                    _libraryPath, AppConstants.Validation.MaxPersistedFileBytes, cancellationToken) ?? [];
                if (items.Count > AppConstants.Validation.MaxLibraryItems)
                {
                    throw new InvalidDataException($"Library data exceeds the {AppConstants.Validation.MaxLibraryItems}-record limit.");
                }
            }

            var validItems = items.Where(item => MediaItemValidator.Validate(item).IsSuccess).ToList();
            if (validItems.Count != items.Count)
                _logger.LogWarning("Ignored {Count} invalid persisted media records", items.Count - validItems.Count);

            await PopulateArtworkAsync(validItems, cancellationToken);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                MediaItems.Clear();
                foreach (var item in validItems)
                {
                    PopulateFolder(item);
                    MediaItems.Add(item);
                }
                RefreshCollections();
            });

            if (File.Exists(_libraryPath) && validItems.Count > 0)
            {
                await _database.ReplaceMediaAsync(validItems, cancellationToken);
                File.Move(_libraryPath, _libraryPath + ".migrated", overwrite: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading library data");
        }

        // Keep ownership of the background work so cancellation and failures are observable.
        _scanCancellation?.Cancel();
        _scanCancellation?.Dispose();
        _scanCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _deviceScanTask = RunOwnedDeviceScanAsync(_scanCancellation.Token);
    }

    public async Task ScanDeviceMediaAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting device media scan");
            var scanned = await _scanner.ScanAsync(cancellationToken);
            var newItems = scanned.ToList();

            if (newItems.Count == 0)
            {
                _logger.LogInformation("Device scan complete — no media found");
                return;
            }

            await AddItemsAsync(newItems, cancellationToken);
            _logger.LogInformation("Device scan complete — added/merged {Count} items", newItems.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during device media scan");
        }
    }

    public async Task SaveLibraryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var snapshot = MediaItems.Take(AppConstants.Validation.MaxLibraryItems).ToList();
            await _database.ReplaceMediaAsync(snapshot, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving library data");
        }
    }

    public async Task AddItemsAsync(IEnumerable<MediaItem> items, CancellationToken cancellationToken = default)
    {
        // Collect new items first (can run on any thread)
        var toAdd = new List<MediaItem>();
        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (toAdd.Count + MediaItems.Count >= AppConstants.Validation.MaxLibraryItems)
            {
                _logger.LogWarning("Library item limit reached; remaining scan results were ignored");
                break;
            }

            if (MediaItemValidator.Validate(item).IsFailure)
            {
                _logger.LogWarning("Ignored an invalid media record");
                continue;
            }

            if (MediaItems.Any(m => m.FilePath == item.FilePath))
                continue;

            // Only derive folder info from the file path when it hasn't already been
            // set (e.g. by the Android MediaStore scanner using the DATA column).
            // Never run Path.GetDirectoryName on a content:// URI — it produces garbage.
            if (string.IsNullOrEmpty(item.FolderPath) && !string.IsNullOrEmpty(item.FilePath)
                && !item.FilePath.StartsWith("content://"))
            {
                var directory = Path.GetDirectoryName(item.FilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    item.FolderPath = directory;
                    item.FolderName = Path.GetFileName(directory);
                }
            }

            toAdd.Add(item);
        }

        if (toAdd.Count == 0)
        {
            await SaveLibraryAsync(cancellationToken);
            return;
        }

        await PopulateArtworkAsync(toAdd, cancellationToken);

        // ObservableCollections must be mutated on the main thread.
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            foreach (var item in toAdd)
                MediaItems.Add(item);

            RefreshCollections();
        });

        await SaveLibraryAsync(cancellationToken);
    }

    private async Task PopulateArtworkAsync(IEnumerable<MediaItem> items, CancellationToken cancellationToken)
    {
        // Limit concurrency to avoid decoding many full-size video frames at once.
        using var artworkGate = new SemaphoreSlim(3);
        await Task.WhenAll(items.Select(async item =>
        {
            if (!string.IsNullOrEmpty(item.AlbumArtPath) && File.Exists(item.AlbumArtPath) &&
                !item.AlbumArtPath.StartsWith("content://", StringComparison.OrdinalIgnoreCase))
                return;

            await artworkGate.WaitAsync(cancellationToken);
            try
            {
                var artwork = await _albumArtService.GetAlbumArtPathAsync(item, cancellationToken);
                if (!string.IsNullOrEmpty(artwork)) item.AlbumArtPath = artwork;
            }
            finally
            {
                artworkGate.Release();
            }
        }));
    }

    public async Task RemoveItemAsync(MediaItem item, CancellationToken cancellationToken = default)
    {
        MediaItems.Remove(item);
        RefreshCollections();
        await SaveLibraryAsync(cancellationToken);
    }

    public async Task ClearLibraryAsync(CancellationToken cancellationToken = default)
    {
        MediaItems.Clear();
        RefreshCollections();
        await SaveLibraryAsync(cancellationToken);
    }

    public async Task ToggleFavoriteAsync(MediaItem item, CancellationToken cancellationToken = default)
    {
        item.IsFavorite = !item.IsFavorite;
        RefreshSmartPlaylists();
        await SaveLibraryAsync(cancellationToken);
    }

    public async Task RecordPlayAsync(MediaItem item, CancellationToken cancellationToken = default)
    {
        item.PlayCount++;
        item.LastPlayedDate = DateTime.Now;
        RefreshSmartPlaylists();
        await SaveLibraryAsync(cancellationToken);
    }

    public void RefreshCollections()
    {
        RefreshSongsAndVideos();
        RefreshArtists();
        RefreshAlbums();
        RefreshFolders();
        RefreshSmartPlaylists();
    }

    private void RefreshSongsAndVideos()
    {
        Songs.Clear();
        Videos.Clear();

        foreach (var item in MediaItems)
        {
            if (item.MediaType == MediaType.Audio)
                Songs.Add(item);
            else if (item.MediaType == MediaType.Video)
                Videos.Add(item);
        }
    }

    private async Task RunOwnedDeviceScanAsync(CancellationToken cancellationToken)
    {
        try
        {
            await ScanDeviceMediaAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("Device scan cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Background device scan failed");
        }
    }

    private static void PopulateFolder(MediaItem item)
    {
        if (!string.IsNullOrEmpty(item.FolderPath) || string.IsNullOrEmpty(item.FilePath) ||
            item.FilePath.StartsWith("content://", StringComparison.OrdinalIgnoreCase)) return;

        var directory = Path.GetDirectoryName(item.FilePath);
        if (string.IsNullOrEmpty(directory)) return;
        item.FolderPath = directory;
        item.FolderName = Path.GetFileName(directory);
    }

    public void Dispose()
    {
        _scanCancellation?.Cancel();
        _scanCancellation?.Dispose();
    }

    private void RefreshArtists()
    {
        Artists.Clear();

        var artistGroups = Songs
            .GroupBy(s => s.Artist)
            .OrderBy(g => g.Key);

        foreach (var group in artistGroups)
        {
            var artist = new Artist
            {
                Name = group.Key,
                Songs = new ObservableCollection<MediaItem>(group.OrderBy(s => s.Album).ThenBy(s => s.Title))
            };
            Artists.Add(artist);
        }
    }

    private void RefreshAlbums()
    {
        Albums.Clear();

        var albumGroups = Songs
            .GroupBy(s => new { s.Album, s.Artist })
            .OrderBy(g => g.Key.Album);

        foreach (var group in albumGroups)
        {
            var firstSong = group.First();
            var album = new Album
            {
                Name = group.Key.Album,
                ArtistName = group.Key.Artist,
                Year = firstSong.Year,
                CoverArtPath = firstSong.AlbumArtPath,
                Songs = new ObservableCollection<MediaItem>(group.OrderBy(s => s.Title))
            };
            Albums.Add(album);
        }
    }

    private void RefreshFolders()
    {
        Folders.Clear();

        var folderGroups = MediaItems
            .Where(m => !string.IsNullOrEmpty(m.FolderPath))
            .GroupBy(m => m.FolderPath)
            .OrderBy(g => g.First().FolderName);

        foreach (var group in folderGroups)
        {
            var firstItem = group.First();
            var folder = new Folder
            {
                Path = group.Key,
                Name = firstItem.FolderName,
                Items = new ObservableCollection<MediaItem>(group.OrderBy(m => m.Title))
            };
            Folders.Add(folder);
        }
    }

    private void RefreshSmartPlaylists()
    {
        foreach (var playlist in SmartPlaylists)
        {
            playlist.Items.Clear();

            var items = playlist.PlaylistType switch
            {
                PlaylistType.LikedSongs => MediaItems
                    .Where(m => m.IsFavorite && m.MediaType == MediaType.Audio)
                    .OrderByDescending(m => m.DateAdded),

                PlaylistType.RecentlyPlayed => MediaItems
                    .Where(m => m.LastPlayedDate.HasValue && m.MediaType == MediaType.Audio)
                    .OrderByDescending(m => m.LastPlayedDate)
                    .Take(50),

                PlaylistType.MostPlayed => MediaItems
                    .Where(m => m.PlayCount > 0 && m.MediaType == MediaType.Audio)
                    .OrderByDescending(m => m.PlayCount)
                    .Take(50),

                PlaylistType.WithLyrics => MediaItems
                    .Where(m => m.HasLyrics && m.MediaType == MediaType.Audio)
                    .OrderBy(m => m.Title),

                _ => Enumerable.Empty<MediaItem>()
            };

            foreach (var item in items)
            {
                playlist.Items.Add(item);
            }
        }
    }
}
