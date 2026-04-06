using Android.Content;
using Android.Provider;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.Platforms.Android.Services;

public class MediaScannerService : IMediaScannerService
{
    public async Task<IEnumerable<MediaItem>> ScanAsync(CancellationToken cancellationToken = default)
    {
        // Permissions must be requested on the main thread — invoke there regardless of caller thread.
        bool granted = await MainThread.InvokeOnMainThreadAsync(EnsurePermissionsAsync);
        if (!granted)
            return [];

        var audio = await ScanAudioAsync(cancellationToken);
        var video = await ScanVideoAsync(cancellationToken);
        return audio.Concat(video);
    }

    // ── Permissions ────────────────────────────────────────────────────────────

    private static async Task<bool> EnsurePermissionsAsync()
    {
        try
        {
            PermissionStatus status;

            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.Tiramisu)
            {
                // Android 13+: granular media permissions
                status = await Permissions.RequestAsync<ReadMediaAudioPermission>();
                if (status != PermissionStatus.Granted) return false;

                status = await Permissions.RequestAsync<ReadMediaVideoPermission>();
                if (status != PermissionStatus.Granted) return false;
            }
            else
            {
                // Android 6–12
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
                if (status != PermissionStatus.Granted) return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    // ── Audio ──────────────────────────────────────────────────────────────────

    private static Task<List<MediaItem>> ScanAudioAsync(CancellationToken ct) => Task.Run(() =>
    {
        var items = new List<MediaItem>();
        var context = global::Android.App.Application.Context;

        string[] projection =
        [
            MediaStore.Audio.Media.InterfaceConsts.Id,
            MediaStore.Audio.Media.InterfaceConsts.Title,
            MediaStore.Audio.Media.InterfaceConsts.Artist,
            MediaStore.Audio.Media.InterfaceConsts.Album,
            MediaStore.Audio.Media.InterfaceConsts.AlbumArtist,
            MediaStore.Audio.Media.InterfaceConsts.Duration,
            MediaStore.Audio.Media.InterfaceConsts.Year,
            MediaStore.Audio.Media.InterfaceConsts.Genre,
            MediaStore.Audio.Media.InterfaceConsts.Data,
            MediaStore.Audio.Media.InterfaceConsts.AlbumId,
        ];

        using var cursor = context.ContentResolver?.Query(
            MediaStore.Audio.Media.ExternalContentUri!,
            projection,
            $"{MediaStore.Audio.Media.InterfaceConsts.Duration} > 0",  // skip zero-length
            null,
            $"{MediaStore.Audio.Media.InterfaceConsts.Title} ASC");

        if (cursor == null) return items;

        int colId       = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Id);
        int colTitle    = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Title);
        int colArtist   = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Artist);
        int colAlbum    = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Album);
        int colDuration = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Duration);
        int colYear     = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Year);
        int colData     = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.Data);
        int colAlbumId  = cursor.GetColumnIndex(MediaStore.Audio.Media.InterfaceConsts.AlbumId);

        while (cursor.MoveToNext())
        {
            if (ct.IsCancellationRequested) break;

            var id       = cursor.GetLong(colId);
            var title    = cursor.GetString(colTitle) ?? Path.GetFileNameWithoutExtension("unknown");
            var artist   = NormalizeArtist(cursor.GetString(colArtist));
            var album    = NormalizeAlbum(cursor.GetString(colAlbum));
            var duration = TimeSpan.FromMilliseconds(cursor.GetLong(colDuration));
            var year     = cursor.GetInt(colYear);
            var filePath = cursor.GetString(colData);
            var albumId  = colAlbumId >= 0 ? cursor.GetLong(colAlbumId) : 0L;

            // Album art URI from MediaStore (fast, no disk read)
            var albumArtUri = albumId > 0
                ? ContentUris.WithAppendedId(
                    MediaStore.Audio.Albums.ExternalContentUri!, albumId)?.ToString()
                : null;

            // Use content URI — works on all Android versions
            var contentUri = ContentUris
                .WithAppendedId(MediaStore.Audio.Media.ExternalContentUri!, id)!
                .ToString()!;

            // Prefer content URI; DATA path may be inaccessible on Android 10+
            var itemPath = contentUri;
            var folderPath = string.Empty;
            var folderName = string.Empty;

            if (!string.IsNullOrEmpty(filePath))
            {
                folderPath = Path.GetDirectoryName(filePath) ?? string.Empty;
                folderName = Path.GetFileName(folderPath);
            }

            items.Add(new MediaItem
            {
                Title        = title,
                Artist       = artist,
                Album        = album,
                FilePath     = itemPath,
                FolderPath   = folderPath,
                FolderName   = folderName,
                Duration     = duration,
                Year         = year,
                AlbumArtPath = albumArtUri,
                MediaType    = Tips_Player.Models.MediaType.Audio,
            });
        }

        return items;
    }, ct);

    // ── Video ──────────────────────────────────────────────────────────────────

    private static Task<List<MediaItem>> ScanVideoAsync(CancellationToken ct) => Task.Run(() =>
    {
        var items = new List<MediaItem>();
        var context = global::Android.App.Application.Context;

        string[] projection =
        [
            MediaStore.Video.Media.InterfaceConsts.Id,
            MediaStore.Video.Media.InterfaceConsts.Title,
            MediaStore.Video.Media.InterfaceConsts.Duration,
            MediaStore.Video.Media.InterfaceConsts.Year,
            MediaStore.Video.Media.InterfaceConsts.Data,
        ];

        using var cursor = context.ContentResolver?.Query(
            MediaStore.Video.Media.ExternalContentUri!,
            projection,
            $"{MediaStore.Video.Media.InterfaceConsts.Duration} > 0",
            null,
            $"{MediaStore.Video.Media.InterfaceConsts.Title} ASC");

        if (cursor == null) return items;

        int colId       = cursor.GetColumnIndex(MediaStore.Video.Media.InterfaceConsts.Id);
        int colTitle    = cursor.GetColumnIndex(MediaStore.Video.Media.InterfaceConsts.Title);
        int colDuration = cursor.GetColumnIndex(MediaStore.Video.Media.InterfaceConsts.Duration);
        int colYear     = cursor.GetColumnIndex(MediaStore.Video.Media.InterfaceConsts.Year);
        int colData     = cursor.GetColumnIndex(MediaStore.Video.Media.InterfaceConsts.Data);

        while (cursor.MoveToNext())
        {
            if (ct.IsCancellationRequested) break;

            var id       = cursor.GetLong(colId);
            var title    = cursor.GetString(colTitle) ?? "Unknown";
            var duration = TimeSpan.FromMilliseconds(cursor.GetLong(colDuration));
            var year     = cursor.GetInt(colYear);
            var filePath = cursor.GetString(colData);

            var contentUri = ContentUris
                .WithAppendedId(MediaStore.Video.Media.ExternalContentUri!, id)!
                .ToString()!;

            var folderPath = string.Empty;
            var folderName = string.Empty;
            if (!string.IsNullOrEmpty(filePath))
            {
                folderPath = Path.GetDirectoryName(filePath) ?? string.Empty;
                folderName = Path.GetFileName(folderPath);
            }

            items.Add(new MediaItem
            {
                Title      = title,
                Artist     = "Unknown Artist",
                Album      = "Unknown Album",
                FilePath   = contentUri,
                FolderPath = folderPath,
                FolderName = folderName,
                Duration   = duration,
                Year       = year,
                MediaType  = Tips_Player.Models.MediaType.Video,
            });
        }

        return items;
    }, ct);

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static string NormalizeArtist(string? raw) =>
        string.IsNullOrWhiteSpace(raw) || raw == "<unknown>" ? "Unknown Artist" : raw;

    private static string NormalizeAlbum(string? raw) =>
        string.IsNullOrWhiteSpace(raw) || raw == "<unknown>" ? "Unknown Album" : raw;
}

// ── Custom MAUI permission wrappers ───────────────────────────────────────────

internal class ReadMediaAudioPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        [("android.permission.READ_MEDIA_AUDIO", true)];
}

internal class ReadMediaVideoPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        [("android.permission.READ_MEDIA_VIDEO", true)];
}
