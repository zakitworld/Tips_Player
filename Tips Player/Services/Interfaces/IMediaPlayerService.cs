using Tips_Player.Models;

namespace Tips_Player.Services.Interfaces;

public interface IMediaPlayerService
{
    MediaItem? CurrentMedia { get; }
    TimeSpan CurrentPosition { get; }
    TimeSpan Duration { get; }
    bool IsPlaying { get; }
    bool IsMuted { get; set; }
    double Volume { get; set; }
    bool IsShuffleEnabled { get; set; }
    bool IsRepeatEnabled { get; set; }

    event EventHandler<MediaItem?>? MediaChanged;
    event EventHandler<TimeSpan>? PositionChanged;
    event EventHandler<bool>? PlaybackStateChanged;
    event EventHandler? MediaEnded;

    Task LoadAsync(MediaItem media, CancellationToken cancellationToken = default);
    Task PlayAsync(CancellationToken cancellationToken = default);
    Task PauseAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default);
    void SetMediaElement(CommunityToolkit.Maui.Views.MediaElement mediaElement);
}
