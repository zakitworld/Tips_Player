using Tips_Player.Models;

namespace Tips_Player.Services.Interfaces;

public interface IFilePickerService
{
    Task<IEnumerable<MediaItem>> PickMediaFilesAsync();
    Task<MediaItem?> PickSingleMediaFileAsync();
}
