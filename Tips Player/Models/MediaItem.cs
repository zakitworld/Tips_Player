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
    private string _album = "Unknown Album";

    [ObservableProperty]
    private string _genre = string.Empty;

    [ObservableProperty]
    private int _year;

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private string _folderPath = string.Empty;

    [ObservableProperty]
    private string _folderName = string.Empty;

    [ObservableProperty]
    private TimeSpan _duration;

    [ObservableProperty]
    private MediaType _mediaType;

    [ObservableProperty]
    private string? _albumArtPath;

    [ObservableProperty]
    private DateTime _dateAdded = DateTime.Now;

    [ObservableProperty]
    private DateTime? _lastPlayedDate;

    [ObservableProperty]
    private int _playCount;

    [ObservableProperty]
    private bool _isFavorite;

    [ObservableProperty]
    private bool _hasLyrics;

    [ObservableProperty]
    private string? _lyrics;

    [ObservableProperty]
    private int _rating; // 0-5 stars

    [ObservableProperty]
    private List<string> _tags = [];

    [ObservableProperty]
    private TimeSpan _lastPosition; // For resume playback

    public string DurationFormatted => Duration.TotalHours >= 1
        ? Duration.ToString(@"hh\:mm\:ss")
        : Duration.ToString(@"mm\:ss");

    public string DisplaySubtitle => MediaType == MediaType.Video
        ? "Video"
        : Artist;

    public string AlbumDisplay => string.IsNullOrEmpty(Album) || Album == "Unknown Album"
        ? Artist
        : $"{Album} • {Artist}";
}
