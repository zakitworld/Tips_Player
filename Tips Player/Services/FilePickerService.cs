using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;
using Tips_Player.Constants;

namespace Tips_Player.Services;

public class FilePickerService : IFilePickerService
{
    private readonly ILogger<FilePickerService> _logger;
    public FilePickerService(ILogger<FilePickerService> logger)
    {
        _logger = logger;
        _logger.LogInformation("FilePickerService initialized");
    }

    private static readonly FilePickerFileType MediaFileTypes = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, ["*.mp3", "*.wav", "*.aac", "*.m4a", "*.flac", "*.ogg", "*.wma",
                                     "*.mp4", "*.avi", "*.mkv", "*.mov", "*.wmv", "*.webm", "*.m4v"] },
            { DevicePlatform.Android, ["audio/mpeg", "audio/x-wav", "audio/aac", "audio/mp4", "audio/flac",
                                       "audio/ogg", "audio/x-ms-wma", "video/mp4", "video/x-msvideo",
                                       "video/x-matroska", "video/quicktime", "video/x-ms-wmv", "video/webm"] }
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
            var mediaItems = (results ?? []).Select(CreateMediaItem).OfType<MediaItem>().ToList();
            _logger.LogInformation("Picked {Count} media files", mediaItems.Count);
            return mediaItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error picking multiple media files");
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
            if (result != null)
            {
                return CreateMediaItem(result);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error picking single media file");
            return null;
        }
    }

    private static async Task RequestPermissionsAsync()
    {
        // Request storage read permission
        var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
        {
            await Permissions.RequestAsync<Permissions.StorageRead>();
        }
    }

    private static MediaItem? CreateMediaItem(FileResult file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var isAudio = FileConstants.AudioExtensions.Contains(extension);
        var isVideo = FileConstants.VideoExtensions.Contains(extension);
        if (!isAudio && !isVideo)
        {
            return null;
        }

        return new MediaItem
        {
            Title = Path.GetFileNameWithoutExtension(file.FileName),
            FilePath = file.FullPath,
            MediaType = isVideo ? MediaType.Video : MediaType.Audio,
            DateAdded = DateTime.Now
        };
    }
}
