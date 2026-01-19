using CommunityToolkit.Mvvm.ComponentModel;

namespace Tips_Player.Models;

public partial class MediaItem : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _artist = "Unknown Artist";

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private TimeSpan _duration;

    [ObservableProperty]
    private MediaType _mediaType;

    [ObservableProperty]
    private string? _albumArtPath;

    [ObservableProperty]
    private DateTime _dateAdded = DateTime.Now;

    public string DurationFormatted => Duration.TotalHours >= 1
        ? Duration.ToString(@"hh\:mm\:ss")
        : Duration.ToString(@"mm\:ss");

    public string DisplaySubtitle => MediaType == MediaType.Video
        ? "Video"
        : Artist;
}
