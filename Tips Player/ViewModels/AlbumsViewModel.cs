using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.ViewModels;

public partial class AlbumsViewModel : BaseViewModel
{
    private readonly IFilePickerService _filePickerService;
    private readonly PlayerViewModel _playerViewModel;
    private readonly ILibraryService _libraryService;

    [ObservableProperty]
    private Album? _selectedAlbum;

    [ObservableProperty]
    private MediaItem? _selectedItem;

    [ObservableProperty]
    private bool _isAlbumDetailVisible;

    [ObservableProperty]
    private Album? _currentAlbum;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Album> _filteredAlbums = [];

    public ObservableCollection<Album> Albums => _libraryService.Albums;

    public bool HasItems => Albums.Count > 0;

    public AlbumsViewModel(IFilePickerService filePickerService, PlayerViewModel playerViewModel, ILibraryService libraryService)
    {
        _filePickerService = filePickerService;
        _playerViewModel = playerViewModel;
        _libraryService = libraryService;
        Title = "Albums";

        Albums.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(HasItems));
            ApplySearch();
        };

        ApplySearch();
    }

    [RelayCommand]
    private async Task AddFilesAsync()
    {
        IsBusy = true;
        try
        {
            var files = await _filePickerService.PickMediaFilesAsync();
            await _libraryService.AddItemsAsync(files);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenAlbum(Album? album)
    {
        if (album == null) return;

        CurrentAlbum = album;
        IsAlbumDetailVisible = true;
        Title = album.Name;
    }

    [RelayCommand]
    private void CloseAlbumDetail()
    {
        IsAlbumDetailVisible = false;
        CurrentAlbum = null;
        Title = "Albums";
    }

    [RelayCommand]
    private async Task PlayItemAsync(MediaItem? item)
    {
        if (item == null || CurrentAlbum == null) return;

        foreach (var song in CurrentAlbum.Songs)
        {
            if (!_playerViewModel.Playlist.Any(m => m.FilePath == song.FilePath))
            {
                _playerViewModel.Playlist.Add(song);
            }
        }

        await _libraryService.RecordPlayAsync(item);
        await _playerViewModel.PlayMediaAsync(item);
        await Shell.Current.GoToAsync("//PlayerPage");
    }

    [RelayCommand]
    private async Task PlayAllAsync()
    {
        if (CurrentAlbum == null || !CurrentAlbum.Songs.Any()) return;

        _playerViewModel.Playlist.Clear();
        foreach (var song in CurrentAlbum.Songs)
        {
            _playerViewModel.Playlist.Add(song);
        }

        var firstItem = CurrentAlbum.Songs.First();
        await _libraryService.RecordPlayAsync(firstItem);
        await _playerViewModel.PlayMediaAsync(firstItem);
        await Shell.Current.GoToAsync("//PlayerPage");
    }

    [RelayCommand]
    private async Task ShufflePlayAsync()
    {
        if (CurrentAlbum == null || !CurrentAlbum.Songs.Any()) return;

        var songs = CurrentAlbum.Songs.ToList();
        var random = new Random();
        for (int i = songs.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (songs[i], songs[j]) = (songs[j], songs[i]);
        }

        _playerViewModel.Playlist.Clear();
        foreach (var song in songs)
        {
            _playerViewModel.Playlist.Add(song);
        }

        var firstItem = songs.First();
        await _libraryService.RecordPlayAsync(firstItem);
        await _playerViewModel.PlayMediaAsync(firstItem);
        await Shell.Current.GoToAsync("//PlayerPage");
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(MediaItem? item)
    {
        if (item != null)
        {
            item.IsFavorite = !item.IsFavorite;
            await _libraryService.SaveLibraryAsync();
            _libraryService.RefreshCollections();
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplySearch();
    }

    private void ApplySearch()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            FilteredAlbums = new ObservableCollection<Album>(Albums);
        }
        else
        {
            var search = SearchText.ToLowerInvariant();
            var filtered = Albums.Where(a =>
                a.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                a.ArtistName.Contains(search, StringComparison.OrdinalIgnoreCase));
            FilteredAlbums = new ObservableCollection<Album>(filtered);
        }
    }

    partial void OnSelectedAlbumChanged(Album? value)
    {
        if (value != null)
        {
            OpenAlbumCommand.Execute(value);
            SelectedAlbum = null;
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
}
