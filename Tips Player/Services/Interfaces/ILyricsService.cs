using Tips_Player.Models;

namespace Tips_Player.Services.Interfaces;

public interface ILyricsService
{
    Task<Lyrics?> GetLyricsAsync(string title, string artist, CancellationToken cancellationToken = default);
    Task<Lyrics?> GetLyricsFromFileAsync(string filePath, CancellationToken cancellationToken = default);
    Task SaveLyricsAsync(MediaItem media, Lyrics lyrics, CancellationToken cancellationToken = default);
    Task<bool> HasCachedLyricsAsync(MediaItem media, CancellationToken cancellationToken = default);
}
