using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly ILibraryService _libraryService;

    // Playback Settings
    [ObservableProperty]
    private double _volume;

    [ObservableProperty]
    private bool _isMuted;

    [ObservableProperty]
    private bool _isShuffleEnabled;

    [ObservableProperty]
    private bool _isRepeatEnabled;

    [ObservableProperty]
    private bool _autoPlayNext;

    [ObservableProperty]
    private bool _rememberPlaybackPosition;

    [ObservableProperty]
    private int _defaultSleepTimerMinutes;

    // Appearance Settings
    [ObservableProperty]
    private string _selectedTheme = "Dark";

    [ObservableProperty]
    private string _selectedAccentColor = "#6366F1";

    // Library Settings
    [ObservableProperty]
    private bool _showFileExtensions;

    [ObservableProperty]
    private string _selectedSortOrder = "Title";

    [ObservableProperty]
    private bool _groupByAlbum;

    // Video Settings
    [ObservableProperty]
    private string _selectedVideoAspect = "Fit";

    [ObservableProperty]
    private bool _autoRotateVideo;

    // Options for pickers
    public ObservableCollection<string> ThemeOptions { get; } = ["Dark", "Light", "System"];
    public ObservableCollection<string> SortOrderOptions { get; } = ["Title", "Artist", "Date Added", "Duration"];
    public ObservableCollection<string> VideoAspectOptions { get; } = ["Fit", "Fill"];
    public ObservableCollection<int> SleepTimerOptions { get; } = [0, 15, 30, 45, 60, 90, 120];
    public ObservableCollection<string> AccentColorOptions { get; } =
    [
        "#6366F1", // Indigo (default)
        "#EC4899", // Pink
        "#EF4444", // Red
        "#F97316", // Orange
        "#EAB308", // Yellow
        "#22C55E", // Green
        "#06B6D4", // Cyan
        "#3B82F6", // Blue
        "#8B5CF6", // Violet
    ];

    // App Info
    public string AppVersion => AppInfo.VersionString;
    public string AppBuild => AppInfo.BuildString;
    public string AppName => AppInfo.Name;

    // Library Stats
    [ObservableProperty]
    private int _totalSongs;

    [ObservableProperty]
    private int _totalVideos;

    [ObservableProperty]
    private int _totalArtists;

    [ObservableProperty]
    private int _totalAlbums;

    public SettingsViewModel(ISettingsService settingsService, ILibraryService libraryService)
    {
        _settingsService = settingsService;
        _libraryService = libraryService;
        Title = "Settings";

        LoadSettings();
        UpdateLibraryStats();

        _libraryService.MediaItems.CollectionChanged += OnMediaItemsCollectionChanged;
    }

    private void OnMediaItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateLibraryStats();
    }

    private void LoadSettings()
    {
        Volume = _settingsService.Volume;
        IsMuted = _settingsService.IsMuted;
        IsShuffleEnabled = _settingsService.IsShuffleEnabled;
        IsRepeatEnabled = _settingsService.IsRepeatEnabled;
        AutoPlayNext = _settingsService.AutoPlayNext;
        RememberPlaybackPosition = _settingsService.RememberPlaybackPosition;
        DefaultSleepTimerMinutes = _settingsService.DefaultSleepTimerMinutes;
        SelectedTheme = _settingsService.Theme;
        SelectedAccentColor = _settingsService.AccentColor;
        ShowFileExtensions = _settingsService.ShowFileExtensions;
        SelectedSortOrder = _settingsService.SortOrder;
        GroupByAlbum = _settingsService.GroupByAlbum;
        SelectedVideoAspect = _settingsService.DefaultVideoAspect;
        AutoRotateVideo = _settingsService.AutoRotateVideo;
    }

    private void UpdateLibraryStats()
    {
        TotalSongs = _libraryService.Songs.Count;
        TotalVideos = _libraryService.Videos.Count;
        TotalArtists = _libraryService.Artists.Count;
        TotalAlbums = _libraryService.Albums.Count;
    }

    partial void OnVolumeChanged(double value)
    {
        _settingsService.Volume = value;
        _settingsService.SaveSettings();
    }

    partial void OnIsMutedChanged(bool value)
    {
        _settingsService.IsMuted = value;
        _settingsService.SaveSettings();
    }

    partial void OnIsShuffleEnabledChanged(bool value)
    {
        _settingsService.IsShuffleEnabled = value;
        _settingsService.SaveSettings();
    }

    partial void OnIsRepeatEnabledChanged(bool value)
    {
        _settingsService.IsRepeatEnabled = value;
        _settingsService.SaveSettings();
    }

    partial void OnAutoPlayNextChanged(bool value)
    {
        _settingsService.AutoPlayNext = value;
        _settingsService.SaveSettings();
    }

    partial void OnRememberPlaybackPositionChanged(bool value)
    {
        _settingsService.RememberPlaybackPosition = value;
        _settingsService.SaveSettings();
    }

    partial void OnDefaultSleepTimerMinutesChanged(int value)
    {
        _settingsService.DefaultSleepTimerMinutes = value;
        _settingsService.SaveSettings();
    }

    partial void OnSelectedThemeChanged(string value)
    {
        _settingsService.Theme = value;
        _settingsService.SaveSettings();
    }

    partial void OnSelectedAccentColorChanged(string value)
    {
        _settingsService.AccentColor = value;
        _settingsService.SaveSettings();
    }

    partial void OnShowFileExtensionsChanged(bool value)
    {
        _settingsService.ShowFileExtensions = value;
        _settingsService.SaveSettings();
    }

    partial void OnSelectedSortOrderChanged(string value)
    {
        _settingsService.SortOrder = value;
        _settingsService.SaveSettings();
    }

    partial void OnGroupByAlbumChanged(bool value)
    {
        _settingsService.GroupByAlbum = value;
        _settingsService.SaveSettings();
    }

    partial void OnSelectedVideoAspectChanged(string value)
    {
        _settingsService.DefaultVideoAspect = value;
        _settingsService.SaveSettings();
    }

    partial void OnAutoRotateVideoChanged(bool value)
    {
        _settingsService.AutoRotateVideo = value;
        _settingsService.SaveSettings();
    }

    [RelayCommand]
    private async Task ResetSettingsAsync()
    {
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Reset Settings",
            "Are you sure you want to reset all settings to their default values?",
            "Reset",
            "Cancel");

        if (confirm)
        {
            _settingsService.ResetToDefaults();
            LoadSettings();
        }
    }

    [RelayCommand]
    private async Task ClearLibraryAsync()
    {
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Clear Library",
            "Are you sure you want to remove all media from your library? This cannot be undone.",
            "Clear",
            "Cancel");

        if (confirm)
        {
            await _libraryService.ClearLibraryAsync();
        }
    }

    [RelayCommand]
    private async Task OpenGitHubAsync()
    {
        try
        {
            await Launcher.OpenAsync(new Uri("https://github.com"));
        }
        catch
        {
            // Handle error silently
        }
    }

    [RelayCommand]
    private async Task SendFeedbackAsync()
    {
        try
        {
            await Launcher.OpenAsync(new Uri("mailto:feedback@tipsplayer.com?subject=Tips Player Feedback"));
        }
        catch
        {
            // Handle error silently
        }
    }

    [RelayCommand]
    private async Task RateAppAsync()
    {
        try
        {
            // Platform-specific store URLs would go here
            await Shell.Current.DisplayAlertAsync("Rate App", "Thank you for using Tips Player!", "OK");
        }
        catch
        {
            // Handle error silently
        }
    }

    [RelayCommand]
    private static async Task NavigateToStatisticsAsync()
    {
        await Shell.Current.GoToAsync("StatisticsPage");
    }

    [RelayCommand]
    private static async Task NavigateToCarModeAsync()
    {
        await Shell.Current.GoToAsync("CarModePage");
    }

    [RelayCommand]
    private static async Task NavigateToEqualizerAsync()
    {
        await Shell.Current.GoToAsync("EqualizerPage");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _libraryService.MediaItems.CollectionChanged -= OnMediaItemsCollectionChanged;
        }

        base.Dispose(disposing);
    }
}
