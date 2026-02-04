using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.ViewModels;

public partial class LibraryViewModel : BaseViewModel
{
    private readonly IFilePickerService _filePickerService;
    private readonly PlayerViewModel _playerViewModel;
    private readonly ILibraryService _libraryService;

    [ObservableProperty]
    private MediaItem? _selectedItem;

    [ObservableProperty]
    private bool _hasItems;

    [ObservableProperty]
    private string _searchText = string.Empty;

    // Stats for hub cards
    [ObservableProperty]
    private int _songsCount;

    [ObservableProperty]
    private int _videosCount;

    [ObservableProperty]
    private int _artistsCount;

    [ObservableProperty]
    private int _albumsCount;

    [ObservableProperty]
    private int _playlistsCount;

    [ObservableProperty]
    private int _foldersCount;

    // Recent items for quick access
    [ObservableProperty]
    private ObservableCollection<MediaItem> _recentItems = [];

    public ObservableCollection<MediaItem> MediaItems => _libraryService.MediaItems;

    [ObservableProperty]
    private ObservableCollection<MediaItem> _filteredItems = [];

    public LibraryViewModel(IFilePickerService filePickerService, PlayerViewModel playerViewModel, ILibraryService libraryService)
    {
        _filePickerService = filePickerService;
        _playerViewModel = playerViewModel;
        _libraryService = libraryService;
        Title = "Your Library";

        MediaItems.CollectionChanged += OnMediaItemsCollectionChanged;

        HasItems = MediaItems.Count > 0;
        UpdateStats();
        ApplySearch();
    }

    private void OnMediaItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        HasItems = MediaItems.Count > 0;
        UpdateStats();
        ApplySearch();
    }

    public void RefreshStats()
    {
        UpdateStats();
    }

    private void UpdateStats()
    {
        SongsCount = _libraryService.Songs.Count;
        VideosCount = _libraryService.Videos.Count;
        ArtistsCount = _libraryService.Artists.Count;
        AlbumsCount = _libraryService.Albums.Count;
        PlaylistsCount = _libraryService.SmartPlaylists.Count;
        FoldersCount = _libraryService.Folders.Count;

        // Get recent items (last 5 played)
        var recent = MediaItems
            .Where(m => m.LastPlayedDate.HasValue)
            .OrderByDescending(m => m.LastPlayedDate)
            .Take(5)
            .ToList();

        RecentItems.Clear();
        foreach (var item in recent)
        {
            RecentItems.Add(item);
        }
    }

    [RelayCommand]
    private async Task AddFilesAsync()
    {
        IsBusy = true;
        try
        {
            var files = await _filePickerService.PickMediaFilesAsync();
            await _libraryService.AddItemsAsync(files);
            UpdateStats();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PlayItemAsync(MediaItem? item)
    {
        if (item == null) return;

        foreach (var mediaItem in MediaItems)
        {
            if (!_playerViewModel.Playlist.Any(m => m.FilePath == mediaItem.FilePath))
            {
                _playerViewModel.Playlist.Add(mediaItem);
            }
        }

        await _playerViewModel.PlayMediaAsync(item);
        await Shell.Current.GoToAsync("//PlayerPage");
    }

    [RelayCommand]
    private async Task RemoveItemAsync(MediaItem? item)
    {
        if (item != null)
        {
            await _libraryService.RemoveItemAsync(item);
            UpdateStats();
        }
    }

    [RelayCommand]
    private async Task ClearLibraryAsync()
    {
        await _libraryService.ClearLibraryAsync();
        UpdateStats();
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(MediaItem? item)
    {
        if (item != null)
        {
            item.IsFavorite = !item.IsFavorite;
            await _libraryService.SaveLibraryAsync();
        }
    }

    // Navigation commands
    [RelayCommand]
    private static async Task NavigateToSongsAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.SongsPage));
    }

    [RelayCommand]
    private static async Task NavigateToVideosAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.VideosPage));
    }

    [RelayCommand]
    private static async Task NavigateToArtistsAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.ArtistsPage));
    }

    [RelayCommand]
    private static async Task NavigateToAlbumsAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.AlbumsPage));
    }

    [RelayCommand]
    private static async Task NavigateToPlaylistsAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.PlaylistsPage));
    }

    [RelayCommand]
    private static async Task NavigateToFoldersAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.FoldersPage));
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplySearch();
    }

    private void ApplySearch()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredItems = new ObservableCollection<MediaItem>(MediaItems);
        }
        else
        {
            var search = SearchText.ToLowerInvariant();
            var filtered = MediaItems.Where(m =>
                m.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                m.Artist.Contains(search, StringComparison.OrdinalIgnoreCase));
            FilteredItems = new ObservableCollection<MediaItem>(filtered);
        }
    }

    partial void OnSelectedItemChanged(MediaItem? value)
    {
        if (value != null)
        {
            PlayItemCommand.Execute(value);
            SelectedItem = null;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            MediaItems.CollectionChanged -= OnMediaItemsCollectionChanged;
        }

        base.Dispose(disposing);
    }
}
