using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.Services;

public class LyricsService : ILyricsService
{
    private const string LyricsCacheFolder = "lyrics_cache";
    private readonly string _cacheDirectory;
    private readonly ILogger<LyricsService> _logger;

    public LyricsService(ILogger<LyricsService> logger)
    {
        _logger = logger;
        _cacheDirectory = Path.Combine(FileSystem.AppDataDirectory, LyricsCacheFolder);
        if (!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
        _logger.LogInformation("LyricsService initialized. Cache directory: {CacheDirectory}", _cacheDirectory);
    }

    public async Task<Lyrics?> GetLyricsAsync(string title, string artist, CancellationToken cancellationToken = default)
    {
        // First check cache
        var cachedLyrics = await GetCachedLyricsAsync(title, artist, cancellationToken);
        if (cachedLyrics != null)
        {
            return cachedLyrics;
        }

        // Try to fetch from online sources (placeholder for API integration)
        // In a real implementation, you would call lyrics APIs like:
        // - Musixmatch API
        // - Genius API
        // - LyricsOvh API

        // For now, return null - user can add lyrics manually
        return null;
    }

    public async Task<Lyrics?> GetLyricsFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check for .lrc file with same name
            var lrcPath = Path.ChangeExtension(filePath, ".lrc");
            if (File.Exists(lrcPath))
            {
                var content = await File.ReadAllTextAsync(lrcPath, cancellationToken);
                return Lyrics.Parse(content);
            }

            // Check for .txt file with same name
            var txtPath = Path.ChangeExtension(filePath, ".txt");
            if (File.Exists(txtPath))
            {
                var content = await File.ReadAllTextAsync(txtPath, cancellationToken);
                return new Lyrics
                {
                    PlainText = content,
                    IsSynced = false
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading lyrics file for {FilePath}", filePath);
        }

        return null;
    }

    public async Task SaveLyricsAsync(MediaItem media, Lyrics lyrics, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileName = GetCacheFileName(media.Title, media.Artist);
            var filePath = Path.Combine(_cacheDirectory, fileName);

            var data = new LyricsData
            {
                Title = lyrics.Title,
                Artist = lyrics.Artist,
                Source = lyrics.Source,
                IsSynced = lyrics.IsSynced,
                PlainText = lyrics.PlainText,
                Lines = lyrics.Lines.Select(l => new LyricLineData
                {
                    TimestampMs = (long)l.Timestamp.TotalMilliseconds,
                    Text = l.Text
                }).ToList()
            };

            var json = JsonSerializer.Serialize(data);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);

            // Update media item
            media.HasLyrics = true;
            media.Lyrics = lyrics.PlainText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving lyrics for {Title} by {Artist}", media.Title, media.Artist);
        }
    }

    public async Task<bool> HasCachedLyricsAsync(MediaItem media, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fileName = GetCacheFileName(media.Title, media.Artist);
        var filePath = Path.Combine(_cacheDirectory, fileName);
        return await Task.FromResult(File.Exists(filePath));
    }

    private async Task<Lyrics?> GetCachedLyricsAsync(string title, string artist, CancellationToken cancellationToken = default)
    {
        try
        {
            var fileName = GetCacheFileName(title, artist);
            var filePath = Path.Combine(_cacheDirectory, fileName);

            if (!File.Exists(filePath)) return null;

            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            var data = JsonSerializer.Deserialize<LyricsData>(json);

            if (data == null) return null;

            var lyrics = new Lyrics
            {
                Title = data.Title,
                Artist = data.Artist,
                Source = data.Source,
                IsSynced = data.IsSynced,
                PlainText = data.PlainText,
                Lines = data.Lines.Select(l => new LyricLine
                {
                    Timestamp = TimeSpan.FromMilliseconds(l.TimestampMs),
                    Text = l.Text
                }).ToList()
            };

            return lyrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cached lyrics for {Title} by {Artist}", title, artist);
        }

        return null;
    }

    private static string GetCacheFileName(string title, string artist)
    {
        var safeName = $"{SanitizeFileName(artist)}_{SanitizeFileName(title)}.json";
        return safeName;
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
    }

    private class LyricsData
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public bool IsSynced { get; set; }
        public string PlainText { get; set; } = string.Empty;
        public List<LyricLineData> Lines { get; set; } = [];
    }

    private class LyricLineData
    {
        public long TimestampMs { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
