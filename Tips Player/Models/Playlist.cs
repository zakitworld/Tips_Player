using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tips_Player.Models;

public partial class Playlist : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MediaItem> _items = [];

    [ObservableProperty]
    private DateTime _createdDate = DateTime.Now;

    [ObservableProperty]
    private int _currentIndex;

    public MediaItem? CurrentItem => CurrentIndex >= 0 && CurrentIndex < Items.Count
        ? Items[CurrentIndex]
        : null;

    public bool HasNext => CurrentIndex < Items.Count - 1;

    public bool HasPrevious => CurrentIndex > 0;

    public void MoveNext()
    {
        if (HasNext)
            CurrentIndex++;
    }

    public void MovePrevious()
    {
        if (HasPrevious)
            CurrentIndex--;
    }

    public void Shuffle()
    {
        var random = new Random();
        var n = Items.Count;
        while (n > 1)
        {
            n--;
            var k = random.Next(n + 1);
            (Items[k], Items[n]) = (Items[n], Items[k]);
        }
        CurrentIndex = 0;
    }
}
