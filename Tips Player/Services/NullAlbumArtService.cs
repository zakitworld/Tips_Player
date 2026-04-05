using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.Services;

/// <summary>No-op album art service for platforms where MediaMetadataRetriever is unavailable.</summary>
public class NullAlbumArtService : IAlbumArtService
{
    public Task<string?> GetAlbumArtPathAsync(MediaItem item, CancellationToken ct = default)
        => Task.FromResult<string?>(null);
}
