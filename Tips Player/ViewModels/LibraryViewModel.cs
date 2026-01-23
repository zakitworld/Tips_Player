using System.Collections.ObjectModel;
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

    public ObservableCollection<MediaItem> MediaItems => _libraryService.MediaItems;

    [ObservableProperty]
    private ObservableCollection<MediaItem> _filteredItems = [];

    public LibraryViewModel(IFilePickerService filePickerService, PlayerViewModel playerViewModel, ILibraryService libraryService)
    {
        _filePickerService = filePickerService;
        _playerViewModel = playerViewModel;
        _libraryService = libraryService;
        Title = "Your Library";

        MediaItems.CollectionChanged += (s, e) =>
        {
            HasItems = MediaItems.Count > 0;
            ApplySearch();
        };

        HasItems = MediaItems.Count > 0;
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
        }
    }

    [RelayCommand]
    private async Task ClearLibraryAsync()
    {
        await _libraryService.ClearLibraryAsync();
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
}
