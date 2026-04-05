using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.Services;

/// <summary>No-op scanner used on platforms where device-level MediaStore is not available (Windows).</summary>
public class NullMediaScannerService : IMediaScannerService
{
    public Task<IEnumerable<MediaItem>> ScanAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Enumerable.Empty<MediaItem>());
}
