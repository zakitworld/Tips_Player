using CommunityToolkit.Mvvm.ComponentModel;

namespace Tips_Player.Models;

public partial class QueueItem : ObservableObject
{
    [ObservableProperty]
    private MediaItem _media = null!;

    [ObservableProperty]
    private int _position;

    [ObservableProperty]
    private bool _isCurrentlyPlaying;

    [ObservableProperty]
    private bool _isUpNext;
}
