using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.ViewModels;

public partial class PlaylistsViewModel : BaseViewModel
{
    private readonly PlayerViewModel _playerViewModel;
    private readonly ILibraryService _libraryService;

    [ObservableProperty]
    private SmartPlaylist? _selectedPlaylist;

    [ObservableProperty]
    private MediaItem? _selectedItem;

    [ObservableProperty]
    private bool _isPlaylistDetailVisible;

    [ObservableProperty]
    private SmartPlaylist? _currentPlaylist;

    public ObservableCollection<SmartPlaylist> SmartPlaylists => _libraryService.SmartPlaylists;

    public PlaylistsViewModel(PlayerViewModel playerViewModel, ILibraryService libraryService)
    {
        _playerViewModel = playerViewModel;
        _libraryService = libraryService;
        Title = "Playlists";
    }

    [RelayCommand]
    private void OpenPlaylist(SmartPlaylist? playlist)
    {
        if (playlist == null) return;

        CurrentPlaylist = playlist;
        IsPlaylistDetailVisible = true;
        Title = playlist.Name;
    }

    [RelayCommand]
    private void ClosePlaylistDetail()
    {
        IsPlaylistDetailVisible = false;
        CurrentPlaylist = null;
        Title = "Playlists";
    }

    [RelayCommand]
    private async Task PlayItemAsync(MediaItem? item)
    {
        if (item == null || CurrentPlaylist == null) return;

        foreach (var song in CurrentPlaylist.Items)
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
        if (CurrentPlaylist == null || !CurrentPlaylist.Items.Any()) return;

        _playerViewModel.Playlist.Clear();
        foreach (var item in CurrentPlaylist.Items)
        {
            _playerViewModel.Playlist.Add(item);
        }

        var firstItem = CurrentPlaylist.Items.First();
        await _libraryService.RecordPlayAsync(firstItem);
        await _playerViewModel.PlayMediaAsync(firstItem);
        await Shell.Current.GoToAsync("//PlayerPage");
    }

    [RelayCommand]
    private async Task ShufflePlayAsync()
    {
        if (CurrentPlaylist == null || !CurrentPlaylist.Items.Any()) return;

        var items = CurrentPlaylist.Items.ToList();
        var random = new Random();
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }

        _playerViewModel.Playlist.Clear();
        foreach (var item in items)
        {
            _playerViewModel.Playlist.Add(item);
        }

        var firstItem = items.First();
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

    partial void OnSelectedPlaylistChanged(SmartPlaylist? value)
    {
        if (value != null)
        {
            OpenPlaylistCommand.Execute(value);
            SelectedPlaylist = null;
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
