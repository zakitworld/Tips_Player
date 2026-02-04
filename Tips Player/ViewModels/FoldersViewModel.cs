using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.ViewModels;

public partial class FoldersViewModel : BaseViewModel
{
    private readonly IFilePickerService _filePickerService;
    private readonly PlayerViewModel _playerViewModel;
    private readonly ILibraryService _libraryService;

    [ObservableProperty]
    private Folder? _selectedFolder;

    [ObservableProperty]
    private MediaItem? _selectedItem;

    [ObservableProperty]
    private bool _isFolderDetailVisible;

    [ObservableProperty]
    private Folder? _currentFolder;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Folder> _filteredFolders = [];

    public ObservableCollection<Folder> Folders => _libraryService.Folders;

    public bool HasItems => Folders.Count > 0;

    public FoldersViewModel(IFilePickerService filePickerService, PlayerViewModel playerViewModel, ILibraryService libraryService)
    {
        _filePickerService = filePickerService;
        _playerViewModel = playerViewModel;
        _libraryService = libraryService;
        Title = "Folders";

        Folders.CollectionChanged += OnFoldersCollectionChanged;

        ApplySearch();
    }

    private void OnFoldersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
    private void OpenFolder(Folder? folder)
    {
        if (folder == null) return;

        CurrentFolder = folder;
        IsFolderDetailVisible = true;
        Title = folder.Name;
    }

    [RelayCommand]
    private void CloseFolderDetail()
    {
        IsFolderDetailVisible = false;
        CurrentFolder = null;
        Title = "Folders";
    }

    [RelayCommand]
    private async Task PlayItemAsync(MediaItem? item)
    {
        if (item == null || CurrentFolder == null) return;

        foreach (var mediaItem in CurrentFolder.Items)
        {
            if (!_playerViewModel.Playlist.Any(m => m.FilePath == mediaItem.FilePath))
            {
                _playerViewModel.Playlist.Add(mediaItem);
            }
        }

        await _libraryService.RecordPlayAsync(item);
        await _playerViewModel.PlayMediaAsync(item);
        await Shell.Current.GoToAsync("//PlayerPage");
    }

    [RelayCommand]
    private async Task PlayAllAsync()
    {
        if (CurrentFolder == null || !CurrentFolder.Items.Any()) return;

        _playerViewModel.Playlist.Clear();
        foreach (var item in CurrentFolder.Items)
        {
            _playerViewModel.Playlist.Add(item);
        }

        var firstItem = CurrentFolder.Items.First();
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
            FilteredFolders = new ObservableCollection<Folder>(Folders);
        }
        else
        {
            var search = SearchText.ToLowerInvariant();
            var filtered = Folders.Where(f =>
                f.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            FilteredFolders = new ObservableCollection<Folder>(filtered);
        }
    }

    partial void OnSelectedFolderChanged(Folder? value)
    {
        if (value != null)
        {
            OpenFolderCommand.Execute(value);
            SelectedFolder = null;
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
            Folders.CollectionChanged -= OnFoldersCollectionChanged;
        }

        base.Dispose(disposing);
    }
}
