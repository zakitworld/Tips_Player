using Tips_Player.Models;
using Tips_Player.Services.Interfaces;
using Microsoft.Maui.ApplicationModel;

namespace Tips_Player.Services;

public class FilePickerService : IFilePickerService
{
    private static readonly string[] AudioExtensions = [".mp3", ".wav", ".aac", ".m4a", ".flac", ".ogg", ".wma"];
    private static readonly string[] VideoExtensions = [".mp4", ".avi", ".mkv", ".mov", ".wmv", ".webm", ".m4v"];

    private static readonly FilePickerFileType MediaFileTypes = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, ["*.mp3", "*.wav", "*.aac", "*.m4a", "*.flac", "*.ogg", "*.wma",
                                     "*.mp4", "*.avi", "*.mkv", "*.mov", "*.wmv", "*.webm", "*.m4v"] },
            { DevicePlatform.Android, ["audio/*", "video/*"] },
            { DevicePlatform.iOS, ["public.audio", "public.movie"] },
            { DevicePlatform.MacCatalyst, ["public.audio", "public.movie"] }
        });

    public async Task<IEnumerable<MediaItem>> PickMediaFilesAsync()
    {
        await RequestPermissionsAsync();
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Select media files",
                FileTypes = MediaFileTypes
            };

            var results = await FilePicker.Default.PickMultipleAsync(options);
            return results.Where(r => r != null).Select(r => CreateMediaItem(r!)).ToList();
        }
        catch (Exception)
        {
            return [];
        }
    }

    public async Task<MediaItem?> PickSingleMediaFileAsync()
    {
        await RequestPermissionsAsync();
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Select a media file",
                FileTypes = MediaFileTypes
            };

            var result = await FilePicker.Default.PickAsync(options);
            return result != null ? CreateMediaItem(result) : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static async Task RequestPermissionsAsync()
    {
        // Request storage read permission
        var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.StorageRead>();
        }

        // For Android 13+ (API 33+), also request media permission
        if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version >= new Version(13, 0))
        {
            var mediaStatus = await Permissions.CheckStatusAsync<Permissions.Media>();
            if (mediaStatus != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.Media>();
            }
        }
    }

    private static MediaItem CreateMediaItem(FileResult file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var mediaType = VideoExtensions.Contains(extension) ? MediaType.Video : MediaType.Audio;

        return new MediaItem
        {
            Title = Path.GetFileNameWithoutExtension(file.FileName),
            FilePath = file.FullPath,
            MediaType = mediaType,
            DateAdded = DateTime.Now
        };
    }
}
