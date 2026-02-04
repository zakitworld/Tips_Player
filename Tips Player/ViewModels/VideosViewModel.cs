using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.ViewModels;

public partial class VideosViewModel : BaseViewModel
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

    public ObservableCollection<MediaItem> Videos => _libraryService.Videos;

    public bool HasItems => Videos.Count > 0;

    public VideosViewModel(IFilePickerService filePickerService, PlayerViewModel playerViewModel, ILibraryService libraryService)
    {
        _filePickerService = filePickerService;
        _playerViewModel = playerViewModel;
        _libraryService = libraryService;
        Title = "Videos";

        Videos.CollectionChanged += OnVideosCollectionChanged;

        ApplySearch();
    }

    private void OnVideosCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasItems));
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

        foreach (var video in Videos)
        {
            if (!_playerViewModel.Playlist.Any(m => m.FilePath == video.FilePath))
            {
                _playerViewModel.Playlist.Add(video);
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
            FilteredItems = new ObservableCollection<MediaItem>(Videos);
        }
        else
        {
            var search = SearchText.ToLowerInvariant();
            var filtered = Videos.Where(m =>
                m.Title.Contains(search, StringComparison.OrdinalIgnoreCase));
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
            Videos.CollectionChanged -= OnVideosCollectionChanged;
        }

        base.Dispose(disposing);
    }
}
