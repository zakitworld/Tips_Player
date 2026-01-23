using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tips_Player.Models;

public partial class Folder : ObservableObject
{
    [ObservableProperty]
    private string _path = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MediaItem> _items = [];

    public int ItemCount => Items.Count;

    public int SongCount => Items.Count(i => i.MediaType == MediaType.Audio);

    public int VideoCount => Items.Count(i => i.MediaType == MediaType.Video);

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

    public string Subtitle
    {
        get
        {
            var parts = new List<string>();
            if (SongCount > 0) parts.Add($"{SongCount} song{(SongCount > 1 ? "s" : "")}");
            if (VideoCount > 0) parts.Add($"{VideoCount} video{(VideoCount > 1 ? "s" : "")}");
            return string.Join(" • ", parts);
        }
    }
}
