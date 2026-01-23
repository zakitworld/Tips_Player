using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tips_Player.Models;

public partial class Album : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _artistName = "Unknown Artist";

    [ObservableProperty]
    private int _year;

    [ObservableProperty]
    private string? _coverArtPath;

    [ObservableProperty]
    private ObservableCollection<MediaItem> _songs = [];

    public int SongCount => Songs.Count;

    public TimeSpan TotalDuration => TimeSpan.FromTicks(Songs.Sum(s => s.Duration.Ticks));

    public string TotalDurationFormatted
    {
        get
        {
            var total = TotalDuration;
            if (total.TotalHours >= 1)
                return $"{(int)total.TotalHours}h {total.Minutes}m";
            return $"{total.Minutes}m {total.Seconds}s";
        }
    }

    public string Subtitle => Year > 0
        ? $"{ArtistName} • {Year}"
        : ArtistName;
}
