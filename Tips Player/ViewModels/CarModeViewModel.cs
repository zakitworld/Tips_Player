using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.ViewModels;

public partial class CarModeViewModel : ObservableObject
{
    private readonly IMediaPlayerService _mediaPlayerService;
    private readonly PlayerViewModel _playerViewModel;

    [ObservableProperty]
    private MediaItem? _currentMedia;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private TimeSpan _position;

    [ObservableProperty]
    private TimeSpan _duration;

    [ObservableProperty]
    private string _positionFormatted = "0:00";

    [ObservableProperty]
    private string _durationFormatted = "0:00";

    [ObservableProperty]
    private double _progressPercent;

    public CarModeViewModel(IMediaPlayerService mediaPlayerService, PlayerViewModel playerViewModel)
    {
        _mediaPlayerService = mediaPlayerService;
        _playerViewModel = playerViewModel;

        _mediaPlayerService.MediaChanged += OnMediaChanged;
        _mediaPlayerService.PlaybackStateChanged += OnPlaybackStateChanged;
        _mediaPlayerService.PositionChanged += OnPositionChanged;

        // Initialize with current state
        CurrentMedia = _mediaPlayerService.CurrentMedia;
        IsPlaying = _mediaPlayerService.IsPlaying;
        Duration = _mediaPlayerService.Duration;
        Position = _mediaPlayerService.CurrentPosition;
        UpdateFormatted();
    }

    private void OnMediaChanged(object? sender, MediaItem? media)
    {
        CurrentMedia = media;
        Duration = _mediaPlayerService.Duration;
        UpdateFormatted();
    }

    private void OnPlaybackStateChanged(object? sender, bool isPlaying)
    {
        IsPlaying = isPlaying;
    }

    private void OnPositionChanged(object? sender, TimeSpan position)
    {
        Position = position;
        Duration = _mediaPlayerService.Duration;
        UpdateFormatted();
    }

    private void UpdateFormatted()
    {
        PositionFormatted = FormatTime(Position);
        DurationFormatted = FormatTime(Duration);
        ProgressPercent = Duration.TotalSeconds > 0 ? (Position.TotalSeconds / Duration.TotalSeconds) * 100 : 0;
    }

    private static string FormatTime(TimeSpan time)
    {
        if (time.TotalHours >= 1)
            return $"{(int)time.TotalHours}:{time.Minutes:D2}:{time.Seconds:D2}";
        return $"{time.Minutes}:{time.Seconds:D2}";
    }

    [RelayCommand]
    private async Task PlayPauseAsync()
    {
        if (IsPlaying)
            await _mediaPlayerService.PauseAsync();
        else
            await _mediaPlayerService.PlayAsync();
    }

    [RelayCommand]
    private async Task NextTrackAsync()
    {
        await _playerViewModel.NextCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task PreviousTrackAsync()
    {
        await _playerViewModel.PreviousCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private static async Task ExitCarModeAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
