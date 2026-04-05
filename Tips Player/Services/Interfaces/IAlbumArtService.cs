using Tips_Player.Models;

namespace Tips_Player.Services.Interfaces;

public interface IAlbumArtService
{
    /// <summary>
    /// Returns a local file path for the album art of the given media item,
    /// extracting and caching it on first call. Returns null if no art is found.
    /// </summary>
    Task<string?> GetAlbumArtPathAsync(MediaItem item, CancellationToken ct = default);
}
