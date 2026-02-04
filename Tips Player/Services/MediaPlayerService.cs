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

    public MediaPlayerService(ILogger<MediaPlayerService> logger)
    {
        _logger = logger;
        _logger.LogInformation("MediaPlayerService initialized");
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
            _mediaElement.MediaEnded -= OnMediaEnded;
            _mediaElement.StateChanged -= OnStateChanged;
        }

        _mediaElement = mediaElement;
        _mediaElement.MediaEnded += OnMediaEnded;
        _mediaElement.StateChanged += OnStateChanged;

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
        MediaEnded?.Invoke(this, EventArgs.Empty);
    }

    private void OnStateChanged(object? sender, MediaStateChangedEventArgs e)
    {
        PlaybackStateChanged?.Invoke(this, e.NewState == MediaElementState.Playing);
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
        _currentMedia = media;
        _mediaElement.Source = MediaSource.FromFile(media.FilePath);
        MediaChanged?.Invoke(this, media);
        await Task.CompletedTask;
    }

    public async Task PlayAsync(CancellationToken cancellationToken = default)
    {
        if (_mediaElement == null) return;
        cancellationToken.ThrowIfCancellationRequested();
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
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_mediaElement == null) return;
        cancellationToken.ThrowIfCancellationRequested();
        _mediaElement.Stop();
        PlaybackStateChanged?.Invoke(this, false);
        await Task.CompletedTask;
    }

    public async Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default)
    {
        if (_mediaElement == null) return;
        cancellationToken.ThrowIfCancellationRequested();
        await _mediaElement.SeekTo(position, cancellationToken);
    }
}
