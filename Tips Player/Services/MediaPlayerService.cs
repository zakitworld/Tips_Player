using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.Services;

public class MediaPlayerService : IMediaPlayerService
{
    private readonly ILogger<MediaPlayerService> _logger;
    private MediaElement? _mediaElement;
    private MediaItem? _currentMedia;
    private System.Timers.Timer? _positionTimer;
    // Set to true when Play() is called before ExoPlayer has finished preparing the source.
    // The MediaOpened handler will call Play() once the element is ready.
    private volatile bool _pendingPlay;

#if ANDROID
    private readonly Tips_Player.Platforms.Android.Services.AudioFocusManager _audioFocus;
    private float _duckMultiplier = 1f;
#endif

    public MediaPlayerService(ILogger<MediaPlayerService> logger)
    {
        _logger = logger;
        _logger.LogInformation("MediaPlayerService initialized");

#if ANDROID
        _audioFocus = new Tips_Player.Platforms.Android.Services.AudioFocusManager();
        _audioFocus.FocusLost  += () => MainThread.BeginInvokeOnMainThread(() => _ = PauseAsync());
        _audioFocus.FocusGained += () => MainThread.BeginInvokeOnMainThread(() =>
        {
            _duckMultiplier = 1f;
            if (_mediaElement != null) _mediaElement.Volume = Volume;
        });
        _audioFocus.Duck += multiplier => MainThread.BeginInvokeOnMainThread(() =>
        {
            _duckMultiplier = multiplier;
            if (_mediaElement != null) _mediaElement.Volume = Volume * multiplier;
        });
#endif
    }

    public MediaItem? CurrentMedia => _currentMedia;
    public TimeSpan CurrentPosition => _mediaElement?.Position ?? TimeSpan.Zero;
    public TimeSpan Duration => _mediaElement?.Duration ?? TimeSpan.Zero;
    public bool IsPlaying => _mediaElement?.CurrentState == MediaElementState.Playing;

    public bool IsMuted
    {
        get => _mediaElement?.ShouldMute ?? false;
        set
        {
            if (_mediaElement != null)
                _mediaElement.ShouldMute = value;
        }
    }

    public double Volume
    {
        get => _mediaElement?.Volume ?? 1.0;
        set
        {
            if (_mediaElement != null)
                _mediaElement.Volume = Math.Clamp(value, 0, 1);
        }
    }

    public bool IsShuffleEnabled { get; set; }
    public bool IsRepeatEnabled { get; set; }

    public event EventHandler<MediaItem?>? MediaChanged;
    public event EventHandler<TimeSpan>? PositionChanged;
    public event EventHandler<bool>? PlaybackStateChanged;
    public event EventHandler? MediaEnded;

    public void SetMediaElement(MediaElement mediaElement)
    {
        if (_mediaElement != null)
        {
            _mediaElement.MediaEnded   -= OnMediaEnded;
            _mediaElement.StateChanged -= OnStateChanged;
            _mediaElement.MediaOpened  -= OnMediaOpened;
            _mediaElement.MediaFailed  -= OnMediaFailed;
        }

        _mediaElement = mediaElement;
        _mediaElement.MediaEnded   += OnMediaEnded;
        _mediaElement.StateChanged += OnStateChanged;
        _mediaElement.MediaOpened  += OnMediaOpened;
        _mediaElement.MediaFailed  += OnMediaFailed;

        StartPositionTimer();
    }

    private void StartPositionTimer()
    {
        _positionTimer?.Stop();
        _positionTimer = new System.Timers.Timer(500);
        _positionTimer.Elapsed += (s, e) =>
        {
            if (_mediaElement != null && IsPlaying)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PositionChanged?.Invoke(this, CurrentPosition);
                });
            }
        };
        _positionTimer.Start();
    }

    private void OnMediaEnded(object? sender, EventArgs e)
    {
        _pendingPlay = false;
        MediaEnded?.Invoke(this, EventArgs.Empty);
    }

    private void OnStateChanged(object? sender, MediaStateChangedEventArgs e)
    {
        PlaybackStateChanged?.Invoke(this, e.NewState == MediaElementState.Playing);
    }

    // Fired by ExoPlayer once the source is fully prepared and ready to play.
    private void OnMediaOpened(object? sender, EventArgs e)
    {
        if (!_pendingPlay) return;
        _pendingPlay = false;
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_mediaElement == null) return;
#if ANDROID
            _audioFocus.RequestFocus();
            Tips_Player.Platforms.Android.MediaServiceBridge.NotifyState(_currentMedia, true);
            Tips_Player.Platforms.Android.MediaPlaybackService.Start();
#endif
            _mediaElement.Play();
            PlaybackStateChanged?.Invoke(this, true);
        });
    }

    private void OnMediaFailed(object? sender, MediaFailedEventArgs e)
    {
        _pendingPlay = false;
        _logger.LogError("Media failed: {Error}", e.ErrorMessage);
        PlaybackStateChanged?.Invoke(this, false);
    }

    public async Task LoadAsync(MediaItem media, CancellationToken cancellationToken = default)
    {
        if (_mediaElement == null)
        {
            _logger.LogWarning("LoadAsync called but MediaElement is not set");
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("Loading media: {Title} from {FilePath}", media.Title, media.FilePath);

        _pendingPlay = false; // clear any stale pending-play from the previous track
        _currentMedia = media;

        // content:// URIs (Android MediaStore) and http(s) require FromUri; local paths use FromFile
        _mediaElement.Source = media.FilePath.StartsWith("content://") || media.FilePath.StartsWith("http")
            ? MediaSource.FromUri(media.FilePath)
            : MediaSource.FromFile(media.FilePath);

        MediaChanged?.Invoke(this, media);
#if ANDROID
        Tips_Player.Platforms.Android.MediaServiceBridge.NotifyState(media, false);
#endif
        await Task.CompletedTask;
    }

    public async Task PlayAsync(CancellationToken cancellationToken = default)
    {
        if (_mediaElement == null) return;
        cancellationToken.ThrowIfCancellationRequested();

        // If the element is still preparing the source, defer Play() until MediaOpened fires.
        var state = _mediaElement.CurrentState;
        if (state == MediaElementState.Opening || state == MediaElementState.Buffering)
        {
            _pendingPlay = true;
            await Task.CompletedTask;
            return;
        }

#if ANDROID
        _audioFocus.RequestFocus();
        Tips_Player.Platforms.Android.MediaServiceBridge.NotifyState(_currentMedia, true);
        Tips_Player.Platforms.Android.MediaPlaybackService.Start();
#endif
        _mediaElement.Play();
        PlaybackStateChanged?.Invoke(this, true);
        await Task.CompletedTask;
    }

    public async Task PauseAsync(CancellationToken cancellationToken = default)
    {
        if (_mediaElement == null) return;
        cancellationToken.ThrowIfCancellationRequested();
        _mediaElement.Pause();
        PlaybackStateChanged?.Invoke(this, false);
#if ANDROID
        Tips_Player.Platforms.Android.MediaServiceBridge.NotifyState(_currentMedia, false);
#endif
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_mediaElement == null) return;
        cancellationToken.ThrowIfCancellationRequested();
        _mediaElement.Stop();
        PlaybackStateChanged?.Invoke(this, false);
#if ANDROID
        _audioFocus.AbandonFocus();
        Tips_Player.Platforms.Android.MediaServiceBridge.NotifyState(null, false);
        Tips_Player.Platforms.Android.MediaPlaybackService.Stop();
#endif
        await Task.CompletedTask;
    }

    public async Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default)
    {
        if (_mediaElement == null) return;
        cancellationToken.ThrowIfCancellationRequested();
        await _mediaElement.SeekTo(position, cancellationToken);
    }
}
