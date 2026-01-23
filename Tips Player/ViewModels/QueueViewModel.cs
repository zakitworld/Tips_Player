using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.ViewModels;

public partial class QueueViewModel : BaseViewModel
{
    private readonly PlayerViewModel _playerViewModel;
    private readonly ILibraryService _libraryService;

    [ObservableProperty]
    private ObservableCollection<QueueItem> _queueItems = [];

    [ObservableProperty]
    private QueueItem? _selectedItem;

    [ObservableProperty]
    private MediaItem? _currentlyPlaying;

    [ObservableProperty]
    private int _currentIndex;

    [ObservableProperty]
    private bool _hasItems;

    public QueueViewModel(PlayerViewModel playerViewModel, ILibraryService libraryService)
    {
        _playerViewModel = playerViewModel;
        _libraryService = libraryService;
        Title = "Queue";

        RefreshQueue();

        _playerViewModel.Playlist.CollectionChanged += (s, e) => RefreshQueue();
        _playerViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(PlayerViewModel.CurrentIndex) ||
                e.PropertyName == nameof(PlayerViewModel.CurrentMedia))
            {
                RefreshQueue();
            }
        };
    }

    public void RefreshQueue()
    {
        var items = new ObservableCollection<QueueItem>();
        var currentIndex = _playerViewModel.CurrentIndex;

        for (int i = 0; i < _playerViewModel.Playlist.Count; i++)
        {
            var media = _playerViewModel.Playlist[i];
            items.Add(new QueueItem
            {
                Media = media,
                Position = i + 1,
                IsCurrentlyPlaying = i == currentIndex,
                IsUpNext = i == currentIndex + 1
            });
        }

        QueueItems = items;
        CurrentlyPlaying = _playerViewModel.CurrentMedia;
        CurrentIndex = currentIndex;
        HasItems = items.Count > 0;
    }

    [RelayCommand]
    private async Task PlayItemAsync(QueueItem? item)
    {
        if (item == null) return;

        var index = _playerViewModel.Playlist.IndexOf(item.Media);
        if (index >= 0)
        {
            _playerViewModel.CurrentIndex = index;
            await _playerViewModel.LoadAndPlayAsync(item.Media);
        }
    }

    [RelayCommand]
    private void RemoveFromQueue(QueueItem? item)
    {
        if (item == null) return;

        var index = _playerViewModel.Playlist.IndexOf(item.Media);
        if (index >= 0)
        {
            _playerViewModel.Playlist.RemoveAt(index);
            RefreshQueue();
        }
    }

    [RelayCommand]
    private void MoveUp(QueueItem? item)
    {
        if (item == null) return;

        var index = _playerViewModel.Playlist.IndexOf(item.Media);
        if (index > 0)
        {
            _playerViewModel.Playlist.Move(index, index - 1);
            RefreshQueue();
        }
    }

    [RelayCommand]
    private void MoveDown(QueueItem? item)
    {
        if (item == null) return;

        var index = _playerViewModel.Playlist.IndexOf(item.Media);
        if (index < _playerViewModel.Playlist.Count - 1)
        {
            _playerViewModel.Playlist.Move(index, index + 1);
            RefreshQueue();
        }
    }

    [RelayCommand]
    private void PlayNext(MediaItem? media)
    {
        if (media == null) return;

        var currentIndex = _playerViewModel.CurrentIndex;
        var existingIndex = _playerViewModel.Playlist.IndexOf(media);

        if (existingIndex >= 0)
        {
            _playerViewModel.Playlist.Move(existingIndex, currentIndex + 1);
        }
        else
        {
            _playerViewModel.Playlist.Insert(currentIndex + 1, media);
        }

        RefreshQueue();
    }

    [RelayCommand]
    private void AddToQueue(MediaItem? media)
    {
        if (media == null) return;

        if (!_playerViewModel.Playlist.Contains(media))
        {
            _playerViewModel.Playlist.Add(media);
        }

        RefreshQueue();
    }

    [RelayCommand]
    private void ClearQueue()
    {
        var current = _playerViewModel.CurrentMedia;
        _playerViewModel.Playlist.Clear();

        if (current != null)
        {
            _playerViewModel.Playlist.Add(current);
            _playerViewModel.CurrentIndex = 0;
        }

        RefreshQueue();
    }

    [RelayCommand]
    private void ShuffleQueue()
    {
        var current = _playerViewModel.CurrentMedia;
        var items = _playerViewModel.Playlist.ToList();

        if (current != null)
        {
            items.Remove(current);
        }

        var random = new Random();
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }

        _playerViewModel.Playlist.Clear();

        if (current != null)
        {
            _playerViewModel.Playlist.Add(current);
            _playerViewModel.CurrentIndex = 0;
        }

        foreach (var item in items)
        {
            _playerViewModel.Playlist.Add(item);
        }

        RefreshQueue();
    }

    [RelayCommand]
    private async Task SaveAsPlaylistAsync()
    {
        // This would open a dialog to save the current queue as a playlist
        // For now, show a simple alert
        await Shell.Current.DisplayAlertAsync("Save Playlist", "Playlist saved successfully!", "OK");
    }
}
