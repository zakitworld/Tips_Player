using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.ViewModels;

public partial class PlayerViewModel : BaseViewModel
{
    private readonly IMediaPlayerService _mediaPlayerService;
    private readonly IFilePickerService _filePickerService;
    private bool _isUserSeeking;

    [ObservableProperty]
    private MediaItem? _currentMedia;

    [ObservableProperty]
    private ObservableCollection<MediaItem> _playlist = [];

    [ObservableProperty]
    private int _currentIndex = -1;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private TimeSpan _currentPosition;

    [ObservableProperty]
    private TimeSpan _duration;

    [ObservableProperty]
    private double _volume = 1.0;

    [ObservableProperty]
    private bool _isMuted;

    [ObservableProperty]
    private bool _isShuffleEnabled;

    [ObservableProperty]
    private bool _isRepeatEnabled;

    [ObservableProperty]
    private bool _showVideoPlayer;

    [ObservableProperty]
    private bool _isFullScreen;

    [ObservableProperty]
    private double _sliderPosition;

    public string CurrentPositionFormatted => CurrentPosition.TotalHours >= 1
        ? CurrentPosition.ToString(@"hh\:mm\:ss")
        : CurrentPosition.ToString(@"mm\:ss");

    public string DurationFormatted => Duration.TotalHours >= 1
        ? Duration.ToString(@"hh\:mm\:ss")
        : Duration.ToString(@"mm\:ss");

    public double SliderMaximum => Duration.TotalSeconds > 0 ? Duration.TotalSeconds : 100;

    public PlayerViewModel(IMediaPlayerService mediaPlayerService, IFilePickerService filePickerService)
    {
        _mediaPlayerService = mediaPlayerService;
        _filePickerService = filePickerService;
        Title = "Now Playing";

        _mediaPlayerService.PositionChanged += OnPositionChanged;
        _mediaPlayerService.PlaybackStateChanged += OnPlaybackStateChanged;
        _mediaPlayerService.MediaEnded += OnMediaEnded;
        _mediaPlayerService.MediaChanged += OnMediaChanged;
    }

    public void SetMediaElement(MediaElement mediaElement)
    {
        _mediaPlayerService.SetMediaElement(mediaElement);
    }

    private void OnPositionChanged(object? sender, TimeSpan position)
    {
        if (!_isUserSeeking)
        {
            CurrentPosition = position;
            SliderPosition = position.TotalSeconds;
            OnPropertyChanged(nameof(CurrentPositionFormatted));
        }
    }

    private void OnPlaybackStateChanged(object? sender, bool isPlaying)
    {
        IsPlaying = isPlaying;
    }

    private void OnMediaEnded(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (IsRepeatEnabled && CurrentMedia != null)
            {
                await _mediaPlayerService.SeekAsync(TimeSpan.Zero);
                await _mediaPlayerService.PlayAsync();
            }
            else if (HasNext)
            {
                await NextAsync();
            }
            else
            {
                IsPlaying = false;
            }
        });
    }

    private void OnMediaChanged(object? sender, MediaItem? media)
    {
        CurrentMedia = media;
        ShowVideoPlayer = media?.MediaType == MediaType.Video;
    }

    [RelayCommand]
    private async Task PlayPauseAsync()
    {
        if (IsPlaying)
        {
            await _mediaPlayerService.PauseAsync();
        }
        else
        {
            if (CurrentMedia == null && Playlist.Count > 0)
            {
                CurrentIndex = 0;
                await LoadAndPlayAsync(Playlist[0]);
            }
            else
            {
                await _mediaPlayerService.PlayAsync();
            }
        }
    }

    [RelayCommand]
    private async Task StopAsync()
    {
        await _mediaPlayerService.StopAsync();
    }

    public bool HasNext => CurrentIndex < Playlist.Count - 1;
    public bool HasPrevious => CurrentIndex > 0;

    [RelayCommand]
    private async Task NextAsync()
    {
        if (HasNext)
        {
            CurrentIndex++;
            await LoadAndPlayAsync(Playlist[CurrentIndex]);
        }
    }

    [RelayCommand]
    private async Task PreviousAsync()
    {
        if (CurrentPosition.TotalSeconds > 3)
        {
            await _mediaPlayerService.SeekAsync(TimeSpan.Zero);
        }
        else if (HasPrevious)
        {
            CurrentIndex--;
            await LoadAndPlayAsync(Playlist[CurrentIndex]);
        }
    }

    [RelayCommand]
    private void ToggleShuffle()
    {
        IsShuffleEnabled = !IsShuffleEnabled;
        _mediaPlayerService.IsShuffleEnabled = IsShuffleEnabled;
    }

    [RelayCommand]
    private void ToggleRepeat()
    {
        IsRepeatEnabled = !IsRepeatEnabled;
        _mediaPlayerService.IsRepeatEnabled = IsRepeatEnabled;
    }

    [RelayCommand]
    private void ToggleMute()
    {
        IsMuted = !IsMuted;
        _mediaPlayerService.IsMuted = IsMuted;
    }

    [RelayCommand]
    private void ToggleFullScreen()
    {
        IsFullScreen = !IsFullScreen;
    }

    public void OnSliderDragStarted()
    {
        _isUserSeeking = true;
    }

    public async void OnSliderDragCompleted()
    {
        _isUserSeeking = false;
        var position = TimeSpan.FromSeconds(SliderPosition);
        CurrentPosition = position;
        OnPropertyChanged(nameof(CurrentPositionFormatted));
        await _mediaPlayerService.SeekAsync(position);
    }

    public void OnSliderValueChanged(double newValue)
    {
        if (_isUserSeeking)
        {
            CurrentPosition = TimeSpan.FromSeconds(newValue);
            OnPropertyChanged(nameof(CurrentPositionFormatted));
        }
    }

    public async Task LoadAndPlayAsync(MediaItem media)
    {
        CurrentPosition = TimeSpan.Zero;
        SliderPosition = 0;
        Duration = TimeSpan.Zero;
        OnPropertyChanged(nameof(DurationFormatted));
        OnPropertyChanged(nameof(SliderMaximum));
        OnPropertyChanged(nameof(CurrentPositionFormatted));

        await _mediaPlayerService.LoadAsync(media);
        await _mediaPlayerService.PlayAsync();
    }

    public async Task PlayMediaAsync(MediaItem media)
    {
        var existingIndex = Playlist.IndexOf(media);
        if (existingIndex >= 0)
        {
            CurrentIndex = existingIndex;
        }
        else
        {
            Playlist.Add(media);
            CurrentIndex = Playlist.Count - 1;
        }
        await LoadAndPlayAsync(media);
    }

    public void UpdateDuration(TimeSpan duration)
    {
        Duration = duration;
        if (CurrentMedia != null)
        {
            CurrentMedia.Duration = duration;
        }
        OnPropertyChanged(nameof(DurationFormatted));
        OnPropertyChanged(nameof(SliderMaximum));
    }

    [RelayCommand]
    private async Task AddFilesAsync()
    {
        var files = await _filePickerService.PickMediaFilesAsync();
        foreach (var file in files)
        {
            if (!Playlist.Any(m => m.FilePath == file.FilePath))
            {
                Playlist.Add(file);
            }
        }
    }
}
