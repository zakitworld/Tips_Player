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

    [ObservableProperty]
    private ObservableCollection<MediaItem> _mediaItems = [];

    [ObservableProperty]
    private MediaItem? _selectedItem;

    [ObservableProperty]
    private bool _hasItems;

    public LibraryViewModel(IFilePickerService filePickerService, PlayerViewModel playerViewModel)
    {
        _filePickerService = filePickerService;
        _playerViewModel = playerViewModel;
        Title = "Your Library";

        MediaItems.CollectionChanged += (s, e) => HasItems = MediaItems.Count > 0;
    }

    [RelayCommand]
    private async Task AddFilesAsync()
    {
        IsBusy = true;
        try
        {
            var files = await _filePickerService.PickMediaFilesAsync();
            foreach (var file in files)
            {
                if (!MediaItems.Any(m => m.FilePath == file.FilePath))
                {
                    MediaItems.Add(file);
                }
            }
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
    private void RemoveItem(MediaItem? item)
    {
        if (item != null)
        {
            MediaItems.Remove(item);
        }
    }

    [RelayCommand]
    private void ClearLibrary()
    {
        MediaItems.Clear();
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
