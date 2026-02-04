namespace Tips_Player.Constants;

/// <summary>
/// Timer interval constants used throughout the application.
/// </summary>
public static class TimerConstants
{
    /// <summary>
    /// Interval for position updates during playback (in milliseconds).
    /// </summary>
    public const int PositionUpdateIntervalMs = 500;

    /// <summary>
    /// Interval for lyrics synchronization updates (in milliseconds).
    /// </summary>
    public const int LyricsSyncIntervalMs = 100;

    /// <summary>
    /// Interval for sleep timer countdown (in milliseconds).
    /// </summary>
    public const int SleepTimerIntervalMs = 60000; // 1 minute

    /// <summary>
    /// Delay for auto-hiding fullscreen controls (in milliseconds).
    /// </summary>
    public const int FullscreenControlsHideDelayMs = 3000;

    /// <summary>
    /// Delay before starting media playback after loading (in milliseconds).
    /// </summary>
    public const int MediaLoadDelayMs = 200;

    /// <summary>
    /// Debounce delay for search input (in milliseconds).
    /// </summary>
    public const int SearchDebounceDelayMs = 300;

    /// <summary>
    /// Animation duration for UI transitions (in milliseconds).
    /// </summary>
    public const int AnimationDurationMs = 250;

    /// <summary>
    /// Default timeout for async operations (in milliseconds).
    /// </summary>
    public const int DefaultOperationTimeoutMs = 30000;
}
