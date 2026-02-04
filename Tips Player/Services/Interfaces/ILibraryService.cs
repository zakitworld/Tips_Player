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
    Task LoadLibraryAsync(CancellationToken cancellationToken = default);
    Task SaveLibraryAsync(CancellationToken cancellationToken = default);
    Task AddItemsAsync(IEnumerable<MediaItem> items, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(MediaItem item, CancellationToken cancellationToken = default);
    Task ClearLibraryAsync(CancellationToken cancellationToken = default);
    Task ToggleFavoriteAsync(MediaItem item, CancellationToken cancellationToken = default);

    // Play tracking
    Task RecordPlayAsync(MediaItem item, CancellationToken cancellationToken = default);

    // Refresh collections
    void RefreshCollections();
}
