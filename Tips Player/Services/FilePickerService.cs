using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.Services;

public class FilePickerService : IFilePickerService
{
    private readonly ILogger<FilePickerService> _logger;
    private static readonly string[] AudioExtensions = [".mp3", ".wav", ".aac", ".m4a", ".flac", ".ogg", ".wma"];
    private static readonly string[] VideoExtensions = [".mp4", ".avi", ".mkv", ".mov", ".wmv", ".webm", ".m4v"];

    public FilePickerService(ILogger<FilePickerService> logger)
    {
        _logger = logger;
        _logger.LogInformation("FilePickerService initialized");
    }

    private static readonly FilePickerFileType MediaFileTypes = new(
        new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.WinUI, ["*.mp3", "*.wav", "*.aac", "*.m4a", "*.flac", "*.ogg", "*.wma",
                                     "*.mp4", "*.avi", "*.mkv", "*.mov", "*.wmv", "*.webm", "*.m4v"] }
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
            var mediaItems = results.Where(r => r != null).Select(r => CreateMediaItem(r!)).ToList();
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
                _logger.LogInformation("Picked single media file: {FilePath}", result.FullPath);
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
