namespace Tips_Player.Configuration;

/// <summary>
/// Application settings loaded from configuration.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Logging configuration.
    /// </summary>
    public LoggingSettings Logging { get; set; } = new();

    /// <summary>
    /// Feature flags.
    /// </summary>
    public FeatureFlags Features { get; set; } = new();

    /// <summary>
    /// Performance tuning settings.
    /// </summary>
    public PerformanceSettings Performance { get; set; } = new();
}

/// <summary>
/// Logging-related settings.
/// </summary>
public class LoggingSettings
{
    /// <summary>
    /// Minimum log level.
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// Number of days to retain log files.
    /// </summary>
    public int RetentionDays { get; set; } = 7;

    /// <summary>
    /// Whether to log to file.
    /// </summary>
    public bool EnableFileLogging { get; set; } = true;

    /// <summary>
    /// Whether to log to debug output.
    /// </summary>
    public bool EnableDebugLogging { get; set; } = true;
}

/// <summary>
/// Feature flags for enabling/disabling features.
/// </summary>
public class FeatureFlags
{
    /// <summary>
    /// Enable online lyrics fetching.
    /// </summary>
    public bool EnableOnlineLyrics { get; set; } = false;

    /// <summary>
    /// Enable listening statistics tracking.
    /// </summary>
    public bool EnableStatistics { get; set; } = true;

    /// <summary>
    /// Enable car mode.
    /// </summary>
    public bool EnableCarMode { get; set; } = true;

    /// <summary>
    /// Enable equalizer.
    /// </summary>
    public bool EnableEqualizer { get; set; } = true;

    /// <summary>
    /// Enable crossfade between tracks.
    /// </summary>
    public bool EnableCrossfade { get; set; } = true;
}

/// <summary>
/// Performance tuning settings.
/// </summary>
public class PerformanceSettings
{
    /// <summary>
    /// Maximum items to load at once during library import.
    /// </summary>
    public int BatchImportSize { get; set; } = 100;

    /// <summary>
    /// Enable virtualization for large lists.
    /// </summary>
    public bool EnableVirtualization { get; set; } = true;

    /// <summary>
    /// Cache album art in memory.
    /// </summary>
    public bool CacheAlbumArt { get; set; } = true;

    /// <summary>
    /// Maximum album art cache size in MB.
    /// </summary>
    public int AlbumArtCacheSizeMb { get; set; } = 50;
}
