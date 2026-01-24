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
    private readonly ILibraryService _libraryService;
    private readonly ISettingsService _settingsService;
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

    [ObservableProperty]
    private int _sleepTimerMinutes;

    [ObservableProperty]
    private bool _isSleepTimerActive;

    [ObservableProperty]
    private bool _isCustomTimerVisible;

    [ObservableProperty]
    private string _customTimerMinutes = string.Empty;

    [ObservableProperty]
    private Aspect _videoAspect = Aspect.AspectFit;

    // Playback Speed
    [ObservableProperty]
    private double _playbackSpeed = 1.0;

    // A-B Repeat
    [ObservableProperty]
    private bool _isABRepeatEnabled;

    [ObservableProperty]
    private TimeSpan? _repeatPointA;

    [ObservableProperty]
    private TimeSpan? _repeatPointB;

    [ObservableProperty]
    private bool _isSettingPointA;

    [ObservableProperty]
    private bool _isSettingPointB;

    // Crossfade
    [ObservableProperty]
    private bool _isCrossfadeEnabled;

    [ObservableProperty]
    private int _crossfadeDuration = 5; // seconds

    private System.Timers.Timer? _sleepTimer;

    public string CurrentPositionFormatted => CurrentPosition.TotalHours >= 1
        ? CurrentPosition.ToString(@"hh\:mm\:ss")
        : CurrentPosition.ToString(@"mm\:ss");

    public string DurationFormatted => Duration.TotalHours >= 1
        ? Duration.ToString(@"hh\:mm\:ss")
        : Duration.ToString(@"mm\:ss");

    public double SliderMaximum => Duration.TotalSeconds > 0 ? Duration.TotalSeconds : 100;

    public PlayerViewModel(
        IMediaPlayerService mediaPlayerService,
        IFilePickerService filePickerService,
        ILibraryService libraryService,
        ISettingsService settingsService)
    {
        _mediaPlayerService = mediaPlayerService;
        _filePickerService = filePickerService;
        _libraryService = libraryService;
        _settingsService = settingsService;

        Title = "Now Playing";

        // Load persisted settings
        Volume = _settingsService.Volume;
        IsMuted = _settingsService.IsMuted;
        IsShuffleEnabled = _settingsService.IsShuffleEnabled;
        IsRepeatEnabled = _settingsService.IsRepeatEnabled;

        // Sync with Media Player Service
        _mediaPlayerService.Volume = Volume;
        _mediaPlayerService.IsMuted = IsMuted;
        _mediaPlayerService.IsShuffleEnabled = IsShuffleEnabled;
        _mediaPlayerService.IsRepeatEnabled = IsRepeatEnabled;

        _mediaPlayerService.PositionChanged += OnPositionChanged;
        _mediaPlayerService.PlaybackStateChanged += OnPlaybackStateChanged;
        _mediaPlayerService.MediaEnded += OnMediaEnded;
        _mediaPlayerService.MediaChanged += OnMediaChanged;
    }

    private bool _isInitialized;

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        await _libraryService.LoadLibraryAsync();
        foreach (var item in _libraryService.MediaItems)
        {
            Playlist.Add(item);
        }
    }

    partial void OnVolumeChanged(double value)
    {
        _settingsService.Volume = value;
        _mediaPlayerService.Volume = value;
        _settingsService.SaveSettings();
    }

    partial void OnIsMutedChanged(bool value)
    {
        _settingsService.IsMuted = value;
        _mediaPlayerService.IsMuted = value;
        _settingsService.SaveSettings();
    }

    partial void OnIsShuffleEnabledChanged(bool value)
    {
        _settingsService.IsShuffleEnabled = value;
        _mediaPlayerService.IsShuffleEnabled = value;
        _settingsService.SaveSettings();
    }

    partial void OnIsRepeatEnabledChanged(bool value)
    {
        _settingsService.IsRepeatEnabled = value;
        _mediaPlayerService.IsRepeatEnabled = value;
        _settingsService.SaveSettings();
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

    [RelayCommand]
    private void ToggleAspect()
    {
        VideoAspect = VideoAspect == Aspect.AspectFit ? Aspect.AspectFill : Aspect.AspectFit;
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
        // Add a small delay to ensure the native media source is ready for playback
        await Task.Delay(200);
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
        await _libraryService.AddItemsAsync(files);

        foreach (var file in files)
        {
            if (!Playlist.Any(m => m.FilePath == file.FilePath))
            {
                Playlist.Add(file);
            }
        }
    }

    [RelayCommand]
    private void SetSleepTimer(int minutes)
    {
        _sleepTimer?.Stop();
        _sleepTimer?.Dispose();

        IsCustomTimerVisible = false;

        if (minutes <= 0)
        {
            IsSleepTimerActive = false;
            SleepTimerMinutes = 0;
            return;
        }

        SleepTimerMinutes = minutes;
        IsSleepTimerActive = true;

        _sleepTimer = new System.Timers.Timer(60000); // 1 minute
        _sleepTimer.Elapsed += (s, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SleepTimerMinutes--;
                if (SleepTimerMinutes <= 0)
                {
                    if (IsPlaying)
                    {
                        PlayPauseCommand.Execute(null);
                    }
                    IsSleepTimerActive = false;
                    _sleepTimer?.Stop();
                    _sleepTimer?.Dispose();
                    _sleepTimer = null;
                }
            });
        };
        _sleepTimer.Start();
    }

    [RelayCommand]
    private void ToggleCustomTimer()
    {
        IsCustomTimerVisible = !IsCustomTimerVisible;
        if (IsCustomTimerVisible)
        {
            CustomTimerMinutes = string.Empty;
        }
    }

    [RelayCommand]
    private void SetCustomTimer()
    {
        if (int.TryParse(CustomTimerMinutes, out int minutes) && minutes > 0)
        {
            SetSleepTimer(minutes);
            CustomTimerMinutes = string.Empty;
        }
    }

    // Playback Speed Commands
    public double[] AvailableSpeeds { get; } = [0.25, 0.5, 0.75, 1.0, 1.25, 1.5, 1.75, 2.0];

    [RelayCommand]
    private void SetPlaybackSpeed(double speed)
    {
        PlaybackSpeed = speed;
        // Note: Actual speed change would be applied via MediaElement.Speed property
        OnPropertyChanged(nameof(PlaybackSpeedText));
    }

    [RelayCommand]
    private void IncreaseSpeed()
    {
        var currentIndex = Array.IndexOf(AvailableSpeeds, PlaybackSpeed);
        if (currentIndex < AvailableSpeeds.Length - 1)
        {
            SetPlaybackSpeed(AvailableSpeeds[currentIndex + 1]);
        }
    }

    [RelayCommand]
    private void DecreaseSpeed()
    {
        var currentIndex = Array.IndexOf(AvailableSpeeds, PlaybackSpeed);
        if (currentIndex > 0)
        {
            SetPlaybackSpeed(AvailableSpeeds[currentIndex - 1]);
        }
    }

    [RelayCommand]
    private void ResetSpeed()
    {
        SetPlaybackSpeed(1.0);
    }

    public string PlaybackSpeedText => $"{PlaybackSpeed:0.##}x";

    // A-B Repeat Commands
    [RelayCommand]
    private void SetPointA()
    {
        RepeatPointA = CurrentPosition;
        IsSettingPointA = false;
        IsSettingPointB = true;

        if (RepeatPointB.HasValue && RepeatPointB <= RepeatPointA)
        {
            RepeatPointB = null;
        }
    }

    [RelayCommand]
    private void SetPointB()
    {
        if (!RepeatPointA.HasValue)
        {
            SetPointA();
            return;
        }

        if (CurrentPosition > RepeatPointA)
        {
            RepeatPointB = CurrentPosition;
            IsABRepeatEnabled = true;
            IsSettingPointB = false;
        }
    }

    [RelayCommand]
    private void ClearABRepeat()
    {
        RepeatPointA = null;
        RepeatPointB = null;
        IsABRepeatEnabled = false;
        IsSettingPointA = false;
        IsSettingPointB = false;
    }

    [RelayCommand]
    private void ToggleABRepeat()
    {
        if (!RepeatPointA.HasValue)
        {
            // Start setting point A
            IsSettingPointA = true;
            SetPointA();
        }
        else if (!RepeatPointB.HasValue)
        {
            // Set point B
            SetPointB();
        }
        else
        {
            // Toggle or clear
            if (IsABRepeatEnabled)
            {
                ClearABRepeat();
            }
            else
            {
                IsABRepeatEnabled = true;
            }
        }
    }

    public string ABRepeatStatus
    {
        get
        {
            if (!RepeatPointA.HasValue)
                return "Set A";
            if (!RepeatPointB.HasValue)
                return $"A: {FormatTime(RepeatPointA.Value)} - Set B";
            return $"A: {FormatTime(RepeatPointA.Value)} - B: {FormatTime(RepeatPointB.Value)}";
        }
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.TotalHours >= 1
            ? time.ToString(@"h\:mm\:ss")
            : time.ToString(@"m\:ss");
    }

    // Check A-B repeat during playback
    public void CheckABRepeat()
    {
        if (IsABRepeatEnabled && RepeatPointA.HasValue && RepeatPointB.HasValue)
        {
            if (CurrentPosition >= RepeatPointB.Value)
            {
                _ = _mediaPlayerService.SeekAsync(RepeatPointA.Value);
            }
        }
    }

    // Crossfade
    [RelayCommand]
    private void ToggleCrossfade()
    {
        IsCrossfadeEnabled = !IsCrossfadeEnabled;
    }

    [RelayCommand]
    private void SetCrossfadeDuration(int seconds)
    {
        CrossfadeDuration = Math.Clamp(seconds, 1, 12);
    }

    [RelayCommand]
    private static async Task EnterCarModeAsync()
    {
        await Shell.Current.GoToAsync("CarModePage");
    }

    [RelayCommand]
    private static async Task NavigateToQueueAsync()
    {
        await Shell.Current.GoToAsync("QueuePage");
    }

    [RelayCommand]
    private static async Task NavigateToLyricsAsync()
    {
        await Shell.Current.GoToAsync("LyricsPage");
    }

    [RelayCommand]
    private static async Task NavigateToEqualizerAsync()
    {
        await Shell.Current.GoToAsync("EqualizerPage");
    }
}
