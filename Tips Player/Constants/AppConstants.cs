namespace Tips_Player.Constants;

/// <summary>
/// Application-wide constants.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Library-related constants.
    /// </summary>
    public static class Library
    {
        /// <summary>
        /// Maximum number of items to display in "Top" lists.
        /// </summary>
        public const int TopItemsLimit = 50;

        /// <summary>
        /// Maximum number of recent items to display.
        /// </summary>
        public const int RecentItemsLimit = 5;

        /// <summary>
        /// Default number of items per page for pagination.
        /// </summary>
        public const int DefaultPageSize = 50;
    }

    /// <summary>
    /// Playback-related constants.
    /// </summary>
    public static class Playback
    {
        /// <summary>
        /// Default volume level (0.0 to 1.0).
        /// </summary>
        public const double DefaultVolume = 1.0;

        /// <summary>
        /// Minimum volume level.
        /// </summary>
        public const double MinVolume = 0.0;

        /// <summary>
        /// Maximum volume level.
        /// </summary>
        public const double MaxVolume = 1.0;

        /// <summary>
        /// Time in seconds to restart the track when pressing previous.
        /// </summary>
        public const double RestartThresholdSeconds = 3.0;

        /// <summary>
        /// Minimum crossfade duration in seconds.
        /// </summary>
        public const int MinCrossfadeDuration = 1;

        /// <summary>
        /// Maximum crossfade duration in seconds.
        /// </summary>
        public const int MaxCrossfadeDuration = 12;

        /// <summary>
        /// Default crossfade duration in seconds.
        /// </summary>
        public const int DefaultCrossfadeDuration = 5;

        /// <summary>
        /// Available playback speed options.
        /// </summary>
        public static readonly double[] AvailableSpeeds = [0.25, 0.5, 0.75, 1.0, 1.25, 1.5, 1.75, 2.0];

        /// <summary>
        /// Default playback speed.
        /// </summary>
        public const double DefaultPlaybackSpeed = 1.0;
    }

    /// <summary>
    /// UI-related constants.
    /// </summary>
    public static class UI
    {
        /// <summary>
        /// Default accent color.
        /// </summary>
        public const string DefaultAccentColor = "#6366F1";

        /// <summary>
        /// Default theme name.
        /// </summary>
        public const string DefaultTheme = "Dark";

        /// <summary>
        /// Minimum font size for lyrics display.
        /// </summary>
        public const int MinLyricsFontSize = 12;

        /// <summary>
        /// Maximum font size for lyrics display.
        /// </summary>
        public const int MaxLyricsFontSize = 32;

        /// <summary>
        /// Default font size for lyrics display.
        /// </summary>
        public const int DefaultLyricsFontSize = 18;

        /// <summary>
        /// Font size step for lyrics adjustment.
        /// </summary>
        public const int LyricsFontSizeStep = 2;
    }

    /// <summary>
    /// Validation-related constants.
    /// </summary>
    public static class Validation
    {
        /// <summary>
        /// Maximum file path length.
        /// </summary>
        public const int MaxFilePathLength = 260;

        /// <summary>
        /// Maximum title length.
        /// </summary>
        public const int MaxTitleLength = 255;

        /// <summary>
        /// Maximum artist/album name length.
        /// </summary>
        public const int MaxNameLength = 255;
    }
}
