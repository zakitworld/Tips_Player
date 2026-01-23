using Tips_Player.Models;

namespace Tips_Player.Services.Interfaces;

public interface ILyricsService
{
    Task<Lyrics?> GetLyricsAsync(string title, string artist);
    Task<Lyrics?> GetLyricsFromFileAsync(string filePath);
    Task SaveLyricsAsync(MediaItem media, Lyrics lyrics);
    Task<bool> HasCachedLyricsAsync(MediaItem media);
}
