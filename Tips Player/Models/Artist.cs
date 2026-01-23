using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tips_Player.Models;

public partial class Artist : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _imageUrl;

    [ObservableProperty]
    private ObservableCollection<MediaItem> _songs = [];

    public int SongCount => Songs.Count;

    public int AlbumCount => Songs
        .Where(s => !string.IsNullOrEmpty(s.Album) && s.Album != "Unknown Album")
        .Select(s => s.Album)
        .Distinct()
        .Count();

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
}
