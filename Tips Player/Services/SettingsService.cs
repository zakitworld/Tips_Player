using Microsoft.Extensions.Logging;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.Services;

public class SettingsService : ISettingsService
{
    private readonly ILogger<SettingsService> _logger;

    // Preference Keys
    private const string KeyVolume = "volume";
    private const string KeyMuted = "muted";
    private const string KeyShuffle = "shuffle";
    private const string KeyRepeat = "repeat";
    private const string KeyAutoPlayNext = "auto_play_next";
    private const string KeyRememberPosition = "remember_position";
    private const string KeyDefaultSleepTimer = "default_sleep_timer";
    private const string KeyTheme = "theme";
    private const string KeyAccentColor = "accent_color";
    private const string KeyShowFileExtensions = "show_file_extensions";
    private const string KeySortOrder = "sort_order";
    private const string KeyGroupByAlbum = "group_by_album";
    private const string KeyDefaultVideoAspect = "default_video_aspect";
    private const string KeyAutoRotateVideo = "auto_rotate_video";

    // Playback Settings
    public double Volume { get; set; } = 1.0;
    public bool IsMuted { get; set; }
    public bool IsShuffleEnabled { get; set; }
    public bool IsRepeatEnabled { get; set; }
    public bool AutoPlayNext { get; set; } = true;
    public bool RememberPlaybackPosition { get; set; } = true;
    public int DefaultSleepTimerMinutes { get; set; }

    // Appearance Settings
    public string Theme { get; set; } = "Dark";
    public string AccentColor { get; set; } = "#6366F1";

    // Library Settings
    public bool ShowFileExtensions { get; set; }
    public string SortOrder { get; set; } = "Title";
    public bool GroupByAlbum { get; set; } = true;

    // Video Settings
    public string DefaultVideoAspect { get; set; } = "Fit";
    public bool AutoRotateVideo { get; set; } = true;

    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;
        _logger.LogInformation("SettingsService initialized");
        LoadSettings();
    }

    public void LoadSettings()
    {
        // Playback
        Volume = Preferences.Default.Get(KeyVolume, 1.0);
        IsMuted = Preferences.Default.Get(KeyMuted, false);
        IsShuffleEnabled = Preferences.Default.Get(KeyShuffle, false);
        IsRepeatEnabled = Preferences.Default.Get(KeyRepeat, false);
        AutoPlayNext = Preferences.Default.Get(KeyAutoPlayNext, true);
        RememberPlaybackPosition = Preferences.Default.Get(KeyRememberPosition, true);
        DefaultSleepTimerMinutes = Preferences.Default.Get(KeyDefaultSleepTimer, 0);

        // Appearance
        Theme = Preferences.Default.Get(KeyTheme, "Dark");
        AccentColor = Preferences.Default.Get(KeyAccentColor, "#6366F1");

        // Library
        ShowFileExtensions = Preferences.Default.Get(KeyShowFileExtensions, false);
        SortOrder = Preferences.Default.Get(KeySortOrder, "Title");
        GroupByAlbum = Preferences.Default.Get(KeyGroupByAlbum, true);

        // Video
        DefaultVideoAspect = Preferences.Default.Get(KeyDefaultVideoAspect, "Fit");
        AutoRotateVideo = Preferences.Default.Get(KeyAutoRotateVideo, true);
    }

    public void SaveSettings()
    {
        // Playback
        Preferences.Default.Set(KeyVolume, Volume);
        Preferences.Default.Set(KeyMuted, IsMuted);
        Preferences.Default.Set(KeyShuffle, IsShuffleEnabled);
        Preferences.Default.Set(KeyRepeat, IsRepeatEnabled);
        Preferences.Default.Set(KeyAutoPlayNext, AutoPlayNext);
        Preferences.Default.Set(KeyRememberPosition, RememberPlaybackPosition);
        Preferences.Default.Set(KeyDefaultSleepTimer, DefaultSleepTimerMinutes);

        // Appearance
        Preferences.Default.Set(KeyTheme, Theme);
        Preferences.Default.Set(KeyAccentColor, AccentColor);

        // Library
        Preferences.Default.Set(KeyShowFileExtensions, ShowFileExtensions);
        Preferences.Default.Set(KeySortOrder, SortOrder);
        Preferences.Default.Set(KeyGroupByAlbum, GroupByAlbum);

        // Video
        Preferences.Default.Set(KeyDefaultVideoAspect, DefaultVideoAspect);
        Preferences.Default.Set(KeyAutoRotateVideo, AutoRotateVideo);
    }

    public void ResetToDefaults()
    {
        Preferences.Default.Clear();

        Volume = 1.0;
        IsMuted = false;
        IsShuffleEnabled = false;
        IsRepeatEnabled = false;
        AutoPlayNext = true;
        RememberPlaybackPosition = true;
        DefaultSleepTimerMinutes = 0;
        Theme = "Dark";
        AccentColor = "#6366F1";
        ShowFileExtensions = false;
        SortOrder = "Title";
        GroupByAlbum = true;
        DefaultVideoAspect = "Fit";
        AutoRotateVideo = true;

        SaveSettings();
    }
}
