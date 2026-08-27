using Android.Media;
using Bitmap = Android.Graphics.Bitmap;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.Platforms.Android.Services;

public class AlbumArtService : IAlbumArtService
{
    private readonly string _cacheDir;

    public AlbumArtService()
    {
        _cacheDir = Path.Combine(FileSystem.CacheDirectory, "album_art");
        Directory.CreateDirectory(_cacheDir);
    }

    public async Task<string?> GetAlbumArtPathAsync(MediaItem item, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(item.FilePath)) return null;

        // Return cached art if it exists
        var cacheFile = Path.Combine(_cacheDir, $"{item.Id}.jpg");
        if (File.Exists(cacheFile)) return cacheFile;

        return await Task.Run(() => ExtractArt(item.FilePath, cacheFile), ct);
    }

    private static string? ExtractArt(string filePath, string cachePath)
    {
        try
        {
            using var retriever = new MediaMetadataRetriever();

            if (filePath.StartsWith("content://"))
            {
                var uri = global::Android.Net.Uri.Parse(filePath)!;
                retriever.SetDataSource(global::Android.App.Application.Context, uri);
            }
            else
            {
                retriever.SetDataSource(filePath);
            }

            if (itemIsVideo(retriever))
            {
                using var frame = retriever.GetFrameAtTime(-1, Option.ClosestSync);
                if (frame == null) return null;
                using var output = File.Create(cachePath);
                frame.Compress(Bitmap.CompressFormat.Jpeg!, 84, output);
            }
            else
            {
                var artBytes = retriever.GetEmbeddedPicture();
                if (artBytes == null || artBytes.Length == 0) return null;
                File.WriteAllBytes(cachePath, artBytes);
            }
            return cachePath;
        }
        catch
        {
            return null;
        }
    }

    private static bool itemIsVideo(MediaMetadataRetriever retriever)
    {
        var hasVideo = retriever.ExtractMetadata(MetadataKey.HasVideo);
        return string.Equals(hasVideo, "yes", StringComparison.OrdinalIgnoreCase) || hasVideo == "1";
    }
}
