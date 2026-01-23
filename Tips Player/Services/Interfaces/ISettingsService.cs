namespace Tips_Player.Services.Interfaces;

public interface ISettingsService
{
    // Playback Settings
    double Volume { get; set; }
    bool IsMuted { get; set; }
    bool IsShuffleEnabled { get; set; }
    bool IsRepeatEnabled { get; set; }
    bool AutoPlayNext { get; set; }
    bool RememberPlaybackPosition { get; set; }
    int DefaultSleepTimerMinutes { get; set; }

    // Appearance Settings
    string Theme { get; set; } // "Dark", "Light", "System"
    string AccentColor { get; set; }

    // Library Settings
    bool ShowFileExtensions { get; set; }
    string SortOrder { get; set; } // "Title", "Artist", "DateAdded", "Duration"
    bool GroupByAlbum { get; set; }

    // Video Settings
    string DefaultVideoAspect { get; set; } // "Fit", "Fill"
    bool AutoRotateVideo { get; set; }

    void LoadSettings();
    void SaveSettings();
    void ResetToDefaults();
}
