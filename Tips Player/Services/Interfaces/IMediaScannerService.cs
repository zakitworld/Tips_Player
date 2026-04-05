using Tips_Player.Models;

namespace Tips_Player.Services.Interfaces;

public interface IMediaScannerService
{
    /// <summary>Scans the device for audio and video files and returns them as MediaItems.</summary>
    Task<IEnumerable<MediaItem>> ScanAsync(CancellationToken cancellationToken = default);
}
