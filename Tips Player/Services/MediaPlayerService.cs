using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;
using Tips_Player.Infrastructure.Validation;

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
    private volatile bool _sourceOpening;
    private TimeSpan? _pendingSeekPosition;
    private TimeSpan _lastPosition;
    private TimeSpan _lastDuration;
    private volatile bool _isPlaying;

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
    // MediaElement wraps thread-affine WinRT objects on Windows. These cached values
    // are safe for background consumers such as lyrics and statistics timers.
    public TimeSpan CurrentPosition => _lastPosition;
    public TimeSpan Duration => _lastDuration;
    public bool IsPlaying => _isPlaying;

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

        // SetMediaElement is always called by a page lifecycle event on the UI thread.
        _lastPosition = mediaElement.Position;
        _lastDuration = mediaElement.Duration;
        _isPlaying = mediaElement.CurrentState == MediaElementState.Playing;

        StartPositionTimer();
    }

    private void StartPositionTimer()
    {
        _positionTimer?.Stop();
        _positionTimer = new System.Timers.Timer(500);
        _positionTimer.Elapsed += (s, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var element = _mediaElement;
                if (element == null) return;

                try
                {
#if WINDOWS
                    if (element.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.MediaPlayerElement nativeElement &&
                        nativeElement.MediaPlayer?.PlaybackSession is { } session)
                    {
                        if (_pendingSeekPosition is { } pendingPosition)
                        {
                            var pendingTarget = session.NaturalDuration > TimeSpan.Zero && pendingPosition > session.NaturalDuration
                                ? session.NaturalDuration
                                : pendingPosition;
                            session.Position = pendingTarget;
                            _lastPosition = pendingTarget;
                            _pendingSeekPosition = null;
                        }

                        _lastPosition = session.Position;
                        if (session.NaturalDuration > TimeSpan.Zero)
                            _lastDuration = session.NaturalDuration;
                        _isPlaying = session.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing;
                    }
                    else
#endif
                    {
                    _lastPosition = element.Position;
                    _lastDuration = element.Duration;
                    _isPlaying = element.CurrentState == MediaElementState.Playing;
                    }
                    PositionChanged?.Invoke(this, _lastPosition);
                }
                catch (System.Runtime.InteropServices.COMException ex)
                {
                    // The native player may be between handlers during fullscreen
                    // handoff. Skip this tick and let the next UI-thread tick retry.
                    _logger.LogDebug(ex, "Media position unavailable during surface transition");
                }
            });
        };
        _positionTimer.Start();
    }

    private void OnMediaEnded(object? sender, EventArgs e)
    {
        _pendingPlay = false;
        _isPlaying = false;
        MediaEnded?.Invoke(this, EventArgs.Empty);
    }

    private void OnStateChanged(object? sender, MediaStateChangedEventArgs e)
    {
        _isPlaying = e.NewState == MediaElementState.Playing;
        PlaybackStateChanged?.Invoke(this, _isPlaying);
    }

    // Fired by ExoPlayer once the source is fully prepared and ready to play.
    private async void OnMediaOpened(object? sender, EventArgs e)
    {
        _sourceOpening = false;
        if (_mediaElement != null)
            _lastDuration = _mediaElement.Duration;
        var seekPosition = _pendingSeekPosition;
        _pendingSeekPosition = null;

        if (seekPosition.HasValue && _mediaElement != null)
        {
            try
            {
                await SeekElementAsync(seekPosition.Value, CancellationToken.None);
                _lastPosition = seekPosition.Value;
                PositionChanged?.Invoke(this, seekPosition.Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not restore media position after opening");
            }
        }

        if (!_pendingPlay) return;
        _pendingPlay = false;
        if (_mediaElement == null) return;
#if ANDROID
        _audioFocus.RequestFocus();
        Tips_Player.Platforms.Android.MediaServiceBridge.NotifyState(_currentMedia, true);
        Tips_Player.Platforms.Android.MediaPlaybackService.Start();
#endif
        _mediaElement.Play();
        _isPlaying = true;
        PlaybackStateChanged?.Invoke(this, true);
    }

    private void OnMediaFailed(object? sender, MediaFailedEventArgs e)
    {
        _sourceOpening = false;
        _pendingSeekPosition = null;
        _pendingPlay = false;
        _logger.LogError("Media failed: {Error}", e.ErrorMessage);
        PlaybackStateChanged?.Invoke(this, false);
    }

    public async Task LoadAsync(MediaItem media, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(media);
        var validation = MediaItemValidator.Validate(media);
        if (validation.IsFailure)
        {
            _logger.LogWarning("Rejected an invalid media source");
            throw new ArgumentException(validation.Error.Message, nameof(media));
        }

        if (_mediaElement == null)
        {
            _logger.LogWarning("LoadAsync called but MediaElement is not set");
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("Loading local media: {Title}", media.Title);

        _pendingPlay = false; // clear any stale pending-play from the previous track
        _pendingSeekPosition = null;
        _sourceOpening = true;
        _lastPosition = TimeSpan.Zero;
        _lastDuration = media.Duration;
        _isPlaying = false;
        _currentMedia = media;

        // Android MediaStore content URIs require FromUri; local paths use FromFile.
        _mediaElement.Source = media.FilePath.StartsWith("content://", StringComparison.OrdinalIgnoreCase)
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
        _isPlaying = true;
        PlaybackStateChanged?.Invoke(this, true);
        await Task.CompletedTask;
    }

    public async Task PauseAsync(CancellationToken cancellationToken = default)
    {
        if (_mediaElement == null) return;
        cancellationToken.ThrowIfCancellationRequested();
        _mediaElement.Pause();
        _isPlaying = false;
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
        _lastPosition = TimeSpan.Zero;
        _isPlaying = false;
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

        if (position < TimeSpan.Zero)
            position = TimeSpan.Zero;
        var duration = _mediaElement.Duration;
        if (duration > TimeSpan.Zero && position > duration)
            position = duration;

        if (_sourceOpening || _mediaElement.CurrentState is MediaElementState.Opening or MediaElementState.Buffering)
        {
            _pendingSeekPosition = position;
            return;
        }

        await SeekElementAsync(position, cancellationToken);
        _lastPosition = position;
        PositionChanged?.Invoke(this, position);
    }

    /// <summary>
    /// Seeks without using CommunityToolkit MediaElement's Windows async mapper.
    /// Toolkit PlatformSeek can resume on a pool thread and then access the
    /// thread-affine WinUI MediaPlayerElement, raising RPC_E_WRONG_THREAD.
    /// </summary>
    private async Task SeekElementAsync(TimeSpan position, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

#if WINDOWS
        const int maxAttempts = 40;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                var completed = await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (_mediaElement?.Handler?.PlatformView is not Microsoft.UI.Xaml.Controls.MediaPlayerElement nativeElement)
                        return false;

                    var session = nativeElement.MediaPlayer?.PlaybackSession;
                    if (session == null)
                        return false;

                    // Do not gate this on CanSeek. WinUI may report false for a
                    // toolkit-backed local source after it is already seekable.
                    var duration = session.NaturalDuration;
                    var target = duration > TimeSpan.Zero && position > duration ? duration : position;
                    session.Position = target;
                    _lastPosition = target;
                    if (duration > TimeSpan.Zero) _lastDuration = duration;
                    _pendingSeekPosition = null;
                    return true;
                });

                if (completed) return;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                _logger.LogDebug(ex, "Native Windows seek not ready; retrying");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogDebug(ex, "Windows playback session not ready; retrying seek");
            }
            await Task.Delay(50, cancellationToken);
        }

        _pendingSeekPosition = position;
#else
        if (_mediaElement != null)
            await _mediaElement.SeekTo(position, cancellationToken);
#endif
    }
}
