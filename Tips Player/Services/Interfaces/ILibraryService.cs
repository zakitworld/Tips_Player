using System.Collections.ObjectModel;
using Tips_Player.Models;

namespace Tips_Player.Services.Interfaces;

public interface ILibraryService
{
    ObservableCollection<MediaItem> MediaItems { get; }

    // Collections
    ObservableCollection<MediaItem> Songs { get; }
    ObservableCollection<MediaItem> Videos { get; }
    ObservableCollection<Artist> Artists { get; }
    ObservableCollection<Album> Albums { get; }
    ObservableCollection<Folder> Folders { get; }
    ObservableCollection<SmartPlaylist> SmartPlaylists { get; }

    // Basic operations
    Task LoadLibraryAsync();
    Task SaveLibraryAsync();
    Task AddItemsAsync(IEnumerable<MediaItem> items);
    Task RemoveItemAsync(MediaItem item);
    Task ClearLibraryAsync();
    Task ToggleFavoriteAsync(MediaItem item);

    // Play tracking
    Task RecordPlayAsync(MediaItem item);

    // Refresh collections
    void RefreshCollections();
}
