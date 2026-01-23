using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tips_Player.Models;

public partial class SmartPlaylist : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private PlaylistType _playlistType;

    [ObservableProperty]
    private ObservableCollection<MediaItem> _items = [];

    [ObservableProperty]
    private DateTime _createdDate = DateTime.Now;

    public int ItemCount => Items.Count;

    public TimeSpan TotalDuration => TimeSpan.FromTicks(Items.Sum(i => i.Duration.Ticks));

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

    public string Subtitle => $"{ItemCount} song{(ItemCount != 1 ? "s" : "")} • {TotalDurationFormatted}";

    public static SmartPlaylist CreateLikedSongs() => new()
    {
        Name = "Liked Songs",
        Icon = "\uf004", // heart
        PlaylistType = PlaylistType.LikedSongs
    };

    public static SmartPlaylist CreateRecentlyPlayed() => new()
    {
        Name = "Recently Played",
        Icon = "\uf1da", // history
        PlaylistType = PlaylistType.RecentlyPlayed
    };

    public static SmartPlaylist CreateMostPlayed() => new()
    {
        Name = "Most Played",
        Icon = "\uf01e", // repeat
        PlaylistType = PlaylistType.MostPlayed
    };

    public static SmartPlaylist CreateWithLyrics() => new()
    {
        Name = "With Lyrics",
        Icon = "\uf031", // font (text)
        PlaylistType = PlaylistType.WithLyrics
    };
}
