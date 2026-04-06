using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.Services;

public class LibraryService : ILibraryService
{
    private const string LibraryFileName = "library.json";
    private readonly string _libraryPath;
    private readonly ILogger<LibraryService> _logger;
    private readonly IMediaScannerService _scanner;

    public ObservableCollection<MediaItem> MediaItems { get; } = [];
    public ObservableCollection<MediaItem> Songs { get; } = [];
    public ObservableCollection<MediaItem> Videos { get; } = [];
    public ObservableCollection<Artist> Artists { get; } = [];
    public ObservableCollection<Album> Albums { get; } = [];
    public ObservableCollection<Folder> Folders { get; } = [];
    public ObservableCollection<SmartPlaylist> SmartPlaylists { get; } = [];

    public LibraryService(ILogger<LibraryService> logger, IMediaScannerService scanner)
    {
        _logger = logger;
        _scanner = scanner;
        _libraryPath = Path.Combine(FileSystem.AppDataDirectory, LibraryFileName);
        _logger.LogInformation("LibraryService initialized. Library path: {LibraryPath}", _libraryPath);
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
            if (File.Exists(_libraryPath))
            {
                var json = await File.ReadAllTextAsync(_libraryPath, cancellationToken);
                var items = JsonSerializer.Deserialize<List<MediaItem>>(json);

                if (items != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        MediaItems.Clear();
                        foreach (var item in items)
                        {
                            // Only fill FolderPath for plain file paths, not content:// URIs
                            if (string.IsNullOrEmpty(item.FolderPath)
                                && !string.IsNullOrEmpty(item.FilePath)
                                && !item.FilePath.StartsWith("content://"))
                            {
                                var directory = Path.GetDirectoryName(item.FilePath);
                                if (!string.IsNullOrEmpty(directory))
                                {
                                    item.FolderPath = directory;
                                    item.FolderName = Path.GetFileName(directory);
                                }
                            }
                            MediaItems.Add(item);
                        }
                        RefreshCollections();
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading library from {LibraryPath}", _libraryPath);
        }

        // 2. Kick off a background device scan — adds new files, skips duplicates
        _ = Task.Run(async () =>
        {
            try
            {
                await ScanDeviceMediaAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Background device scan failed");
            }
        }, cancellationToken);
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
            var json = JsonSerializer.Serialize(MediaItems.ToList());
            await File.WriteAllTextAsync(_libraryPath, json, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving library to {LibraryPath}", _libraryPath);
        }
    }

    public async Task AddItemsAsync(IEnumerable<MediaItem> items, CancellationToken cancellationToken = default)
    {
        // Collect new items first (can run on any thread)
        var toAdd = new List<MediaItem>();
        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

        // ObservableCollections must be mutated on the main thread.
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            foreach (var item in toAdd)
                MediaItems.Add(item);

            RefreshCollections();
        });

        await SaveLibraryAsync(cancellationToken);
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
