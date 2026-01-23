using CommunityToolkit.Mvvm.ComponentModel;

namespace Tips_Player.Models;

public partial class ListeningStats : ObservableObject
{
    [ObservableProperty]
    private TimeSpan _totalListeningTime;

    [ObservableProperty]
    private int _totalTracksPlayed;

    [ObservableProperty]
    private int _totalSessions;

    [ObservableProperty]
    private DateTime _firstListenDate;

    [ObservableProperty]
    private DateTime _lastListenDate;

    [ObservableProperty]
    private List<ArtistStats> _topArtists = [];

    [ObservableProperty]
    private List<AlbumStats> _topAlbums = [];

    [ObservableProperty]
    private List<TrackStats> _topTracks = [];

    [ObservableProperty]
    private Dictionary<int, int> _hourlyListening = []; // Hour -> play count

    [ObservableProperty]
    private Dictionary<DayOfWeek, TimeSpan> _dailyListening = [];

    public string TotalListeningTimeFormatted
    {
        get
        {
            var total = TotalListeningTime;
            if (total.TotalDays >= 1)
                return $"{(int)total.TotalDays}d {total.Hours}h {total.Minutes}m";
            if (total.TotalHours >= 1)
                return $"{(int)total.TotalHours}h {total.Minutes}m";
            return $"{total.Minutes}m {total.Seconds}s";
        }
    }
}

public partial class ArtistStats : ObservableObject
{
    [ObservableProperty]
    private string _artistName = string.Empty;

    [ObservableProperty]
    private int _playCount;

    [ObservableProperty]
    private TimeSpan _totalListenTime;

    [ObservableProperty]
    private int _rank;
}

public partial class AlbumStats : ObservableObject
{
    [ObservableProperty]
    private string _albumName = string.Empty;

    [ObservableProperty]
    private string _artistName = string.Empty;

    [ObservableProperty]
    private int _playCount;

    [ObservableProperty]
    private TimeSpan _totalListenTime;

    [ObservableProperty]
    private int _rank;
}

public partial class TrackStats : ObservableObject
{
    [ObservableProperty]
    private string _trackTitle = string.Empty;

    [ObservableProperty]
    private string _artistName = string.Empty;

    [ObservableProperty]
    private int _playCount;

    [ObservableProperty]
    private TimeSpan _totalListenTime;

    [ObservableProperty]
    private int _rank;
}

public class PlaySession
{
    public string MediaId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public bool Completed { get; set; }
}
