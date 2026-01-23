using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.ViewModels;

public partial class SearchViewModel : BaseViewModel
{
    private readonly ILibraryService _libraryService;
    private readonly PlayerViewModel _playerViewModel;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MediaItem> _songResults = [];

    [ObservableProperty]
    private ObservableCollection<MediaItem> _videoResults = [];

    [ObservableProperty]
    private ObservableCollection<Artist> _artistResults = [];

    [ObservableProperty]
    private ObservableCollection<Album> _albumResults = [];

    [ObservableProperty]
    private ObservableCollection<Folder> _folderResults = [];

    [ObservableProperty]
    private ObservableCollection<SmartPlaylist> _playlistResults = [];

    [ObservableProperty]
    private bool _hasResults;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private ObservableCollection<string> _searchHistory = [];

    [ObservableProperty]
    private MediaItem? _selectedItem;

    public SearchViewModel(ILibraryService libraryService, PlayerViewModel playerViewModel)
    {
        _libraryService = libraryService;
        _playerViewModel = playerViewModel;
        Title = "Search";

        LoadSearchHistory();
    }

    partial void OnSearchQueryChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            ClearResults();
        }
        else
        {
            PerformSearch(value);
        }
    }

    private void PerformSearch(string query)
    {
        IsSearching = true;

        var searchLower = query.ToLowerInvariant();

        // Search songs
        SongResults = new ObservableCollection<MediaItem>(
            _libraryService.Songs.Where(s =>
                s.Title.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ||
                s.Artist.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ||
                s.Album.Contains(searchLower, StringComparison.OrdinalIgnoreCase))
            .Take(20));

        // Search videos
        VideoResults = new ObservableCollection<MediaItem>(
            _libraryService.Videos.Where(v =>
                v.Title.Contains(searchLower, StringComparison.OrdinalIgnoreCase))
            .Take(10));

        // Search artists
        ArtistResults = new ObservableCollection<Artist>(
            _libraryService.Artists.Where(a =>
                a.Name.Contains(searchLower, StringComparison.OrdinalIgnoreCase))
            .Take(10));

        // Search albums
        AlbumResults = new ObservableCollection<Album>(
            _libraryService.Albums.Where(a =>
                a.Name.Contains(searchLower, StringComparison.OrdinalIgnoreCase) ||
                a.ArtistName.Contains(searchLower, StringComparison.OrdinalIgnoreCase))
            .Take(10));

        // Search folders
        FolderResults = new ObservableCollection<Folder>(
            _libraryService.Folders.Where(f =>
                f.Name.Contains(searchLower, StringComparison.OrdinalIgnoreCase))
            .Take(10));

        // Search playlists
        PlaylistResults = new ObservableCollection<SmartPlaylist>(
            _libraryService.SmartPlaylists.Where(p =>
                p.Name.Contains(searchLower, StringComparison.OrdinalIgnoreCase))
            .Take(10));

        HasResults = SongResults.Any() || VideoResults.Any() || ArtistResults.Any() ||
                     AlbumResults.Any() || FolderResults.Any() || PlaylistResults.Any();

        IsSearching = false;
    }

    private void ClearResults()
    {
        SongResults.Clear();
        VideoResults.Clear();
        ArtistResults.Clear();
        AlbumResults.Clear();
        FolderResults.Clear();
        PlaylistResults.Clear();
        HasResults = false;
    }

    [RelayCommand]
    private async Task PlaySongAsync(MediaItem? item)
    {
        if (item == null) return;

        AddToSearchHistory(SearchQuery);

        if (!_playerViewModel.Playlist.Contains(item))
        {
            _playerViewModel.Playlist.Add(item);
        }

        await _playerViewModel.PlayMediaAsync(item);
        await Shell.Current.GoToAsync("//PlayerPage");
    }

    [RelayCommand]
    private async Task PlayVideoAsync(MediaItem? item)
    {
        if (item == null) return;

        AddToSearchHistory(SearchQuery);
        await PlaySongAsync(item);
    }

    [RelayCommand]
    private async Task OpenArtistAsync(Artist? artist)
    {
        if (artist == null) return;

        AddToSearchHistory(SearchQuery);
        await Shell.Current.GoToAsync("//ArtistsPage");
    }

    [RelayCommand]
    private async Task OpenAlbumAsync(Album? album)
    {
        if (album == null) return;

        AddToSearchHistory(SearchQuery);
        await Shell.Current.GoToAsync("//AlbumsPage");
    }

    [RelayCommand]
    private void UseHistoryItem(string? query)
    {
        if (!string.IsNullOrEmpty(query))
        {
            SearchQuery = query;
        }
    }

    [RelayCommand]
    private void ClearHistory()
    {
        SearchHistory.Clear();
        SaveSearchHistory();
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchQuery = string.Empty;
        ClearResults();
    }

    private void AddToSearchHistory(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return;

        SearchHistory.Remove(query);
        SearchHistory.Insert(0, query);

        while (SearchHistory.Count > 10)
        {
            SearchHistory.RemoveAt(SearchHistory.Count - 1);
        }

        SaveSearchHistory();
    }

    private void LoadSearchHistory()
    {
        var history = Preferences.Default.Get("search_history", string.Empty);
        if (!string.IsNullOrEmpty(history))
        {
            var items = history.Split('|', StringSplitOptions.RemoveEmptyEntries);
            SearchHistory = new ObservableCollection<string>(items);
        }
    }

    private void SaveSearchHistory()
    {
        var history = string.Join("|", SearchHistory);
        Preferences.Default.Set("search_history", history);
    }
}
