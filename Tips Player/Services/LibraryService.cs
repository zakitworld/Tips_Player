using System.Collections.ObjectModel;
using System.Text.Json;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.Services;

public class LibraryService : ILibraryService
{
    private const string LibraryFileName = "library.json";
    private readonly string _libraryPath;

    public ObservableCollection<MediaItem> MediaItems { get; } = [];
    public ObservableCollection<MediaItem> Songs { get; } = [];
    public ObservableCollection<MediaItem> Videos { get; } = [];
    public ObservableCollection<Artist> Artists { get; } = [];
    public ObservableCollection<Album> Albums { get; } = [];
    public ObservableCollection<Folder> Folders { get; } = [];
    public ObservableCollection<SmartPlaylist> SmartPlaylists { get; } = [];

    public LibraryService()
    {
        _libraryPath = Path.Combine(FileSystem.AppDataDirectory, LibraryFileName);
        InitializeSmartPlaylists();
    }

    private void InitializeSmartPlaylists()
    {
        SmartPlaylists.Add(SmartPlaylist.CreateLikedSongs());
        SmartPlaylists.Add(SmartPlaylist.CreateRecentlyPlayed());
        SmartPlaylists.Add(SmartPlaylist.CreateMostPlayed());
        SmartPlaylists.Add(SmartPlaylist.CreateWithLyrics());
    }

    public async Task LoadLibraryAsync()
    {
        try
        {
            if (!File.Exists(_libraryPath)) return;

            var json = await File.ReadAllTextAsync(_libraryPath);
            var items = JsonSerializer.Deserialize<List<MediaItem>>(json);

            if (items != null)
            {
                MediaItems.Clear();
                foreach (var item in items)
                {
                    // Set folder path and name from file path
                    if (!string.IsNullOrEmpty(item.FilePath))
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
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading library: {ex.Message}");
        }
    }

    public async Task SaveLibraryAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(MediaItems.ToList());
            await File.WriteAllTextAsync(_libraryPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving library: {ex.Message}");
        }
    }

    public async Task AddItemsAsync(IEnumerable<MediaItem> items)
    {
        foreach (var item in items)
        {
            if (!MediaItems.Any(m => m.FilePath == item.FilePath))
            {
                // Set folder path and name from file path
                if (!string.IsNullOrEmpty(item.FilePath))
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
        }
        RefreshCollections();
        await SaveLibraryAsync();
    }

    public async Task RemoveItemAsync(MediaItem item)
    {
        MediaItems.Remove(item);
        RefreshCollections();
        await SaveLibraryAsync();
    }

    public async Task ClearLibraryAsync()
    {
        MediaItems.Clear();
        RefreshCollections();
        await SaveLibraryAsync();
    }

    public async Task ToggleFavoriteAsync(MediaItem item)
    {
        item.IsFavorite = !item.IsFavorite;
        RefreshSmartPlaylists();
        await SaveLibraryAsync();
    }

    public async Task RecordPlayAsync(MediaItem item)
    {
        item.PlayCount++;
        item.LastPlayedDate = DateTime.Now;
        RefreshSmartPlaylists();
        await SaveLibraryAsync();
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
