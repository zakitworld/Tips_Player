using Tips_Player.Models;

namespace Tips_Player.Platforms.Android;

/// <summary>
/// Static bridge that lets the shared MAUI layer communicate with the Android
/// foreground service without a direct dependency.
/// </summary>
public static class MediaServiceBridge
{
    public static MediaItem? CurrentMedia { get; private set; }
    public static bool IsPlaying { get; private set; }

    // ── App → Service (state updates) ─────────────────────────────────────────
    public static event Action? StateChanged;

    // ── Notification buttons → App (commands) ────────────────────────────────
    public static event Action? PlayPauseRequested;
    public static event Action? NextRequested;
    public static event Action? PreviousRequested;
    public static event Action? StopRequested;

    internal static void NotifyState(MediaItem? media, bool playing)
    {
        CurrentMedia = media;
        IsPlaying    = playing;
        StateChanged?.Invoke();
    }

    internal static void TriggerPlayPause()  => PlayPauseRequested?.Invoke();
    internal static void TriggerNext()        => NextRequested?.Invoke();
    internal static void TriggerPrevious()    => PreviousRequested?.Invoke();
    internal static void TriggerStop()        => StopRequested?.Invoke();
}
