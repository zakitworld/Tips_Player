using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.ViewModels;

public partial class SongsViewModel : BaseViewModel
{
    private readonly IFilePickerService _filePickerService;
    private readonly PlayerViewModel _playerViewModel;
    private readonly ILibraryService _libraryService;

    [ObservableProperty]
    private MediaItem? _selectedItem;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MediaItem> _filteredItems = [];

    public ObservableCollection<MediaItem> Songs => _libraryService.Songs;

    public bool HasItems => Songs.Count > 0;

    public SongsViewModel(IFilePickerService filePickerService, PlayerViewModel playerViewModel, ILibraryService libraryService)
    {
        _filePickerService = filePickerService;
        _playerViewModel = playerViewModel;
        _libraryService = libraryService;
        Title = "Songs";

        Songs.CollectionChanged += (s, e) =>
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
    private async Task PlayItemAsync(MediaItem? item)
    {
        if (item == null) return;

        foreach (var song in Songs)
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
    private async Task RemoveItemAsync(MediaItem? item)
    {
        if (item != null)
        {
            await _libraryService.RemoveItemAsync(item);
        }
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
            FilteredItems = new ObservableCollection<MediaItem>(Songs);
        }
        else
        {
            var search = SearchText.ToLowerInvariant();
            var filtered = Songs.Where(m =>
                m.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                m.Artist.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                m.Album.Contains(search, StringComparison.OrdinalIgnoreCase));
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
}
