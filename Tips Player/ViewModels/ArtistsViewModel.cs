using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.ViewModels;

public partial class ArtistsViewModel : BaseViewModel
{
    private readonly IFilePickerService _filePickerService;
    private readonly PlayerViewModel _playerViewModel;
    private readonly ILibraryService _libraryService;

    [ObservableProperty]
    private Artist? _selectedArtist;

    [ObservableProperty]
    private MediaItem? _selectedItem;

    [ObservableProperty]
    private bool _isArtistDetailVisible;

    [ObservableProperty]
    private Artist? _currentArtist;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Artist> _filteredArtists = [];

    public ObservableCollection<Artist> Artists => _libraryService.Artists;

    public bool HasItems => Artists.Count > 0;

    public ArtistsViewModel(IFilePickerService filePickerService, PlayerViewModel playerViewModel, ILibraryService libraryService)
    {
        _filePickerService = filePickerService;
        _playerViewModel = playerViewModel;
        _libraryService = libraryService;
        Title = "Artists";

        Artists.CollectionChanged += (s, e) =>
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
    private void OpenArtist(Artist? artist)
    {
        if (artist == null) return;

        CurrentArtist = artist;
        IsArtistDetailVisible = true;
        Title = artist.Name;
    }

    [RelayCommand]
    private void CloseArtistDetail()
    {
        IsArtistDetailVisible = false;
        CurrentArtist = null;
        Title = "Artists";
    }

    [RelayCommand]
    private async Task PlayItemAsync(MediaItem? item)
    {
        if (item == null || CurrentArtist == null) return;

        foreach (var song in CurrentArtist.Songs)
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
        if (CurrentArtist == null || !CurrentArtist.Songs.Any()) return;

        _playerViewModel.Playlist.Clear();
        foreach (var song in CurrentArtist.Songs)
        {
            _playerViewModel.Playlist.Add(song);
        }

        var firstItem = CurrentArtist.Songs.First();
        await _libraryService.RecordPlayAsync(firstItem);
        await _playerViewModel.PlayMediaAsync(firstItem);
        await Shell.Current.GoToAsync("//PlayerPage");
    }

    [RelayCommand]
    private async Task ShufflePlayAsync()
    {
        if (CurrentArtist == null || !CurrentArtist.Songs.Any()) return;

        var songs = CurrentArtist.Songs.ToList();
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
            FilteredArtists = new ObservableCollection<Artist>(Artists);
        }
        else
        {
            var search = SearchText.ToLowerInvariant();
            var filtered = Artists.Where(a =>
                a.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            FilteredArtists = new ObservableCollection<Artist>(filtered);
        }
    }

    partial void OnSelectedArtistChanged(Artist? value)
    {
        if (value != null)
        {
            OpenArtistCommand.Execute(value);
            SelectedArtist = null;
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
