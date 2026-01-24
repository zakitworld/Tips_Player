using Tips_Player.Views;

namespace Tips_Player;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute(nameof(PlayerPage), typeof(PlayerPage));
        Routing.RegisterRoute(nameof(LibraryPage), typeof(LibraryPage));
        Routing.RegisterRoute(nameof(CarModePage), typeof(CarModePage));

        // Library sub-pages
        Routing.RegisterRoute(nameof(SongsPage), typeof(SongsPage));
        Routing.RegisterRoute(nameof(VideosPage), typeof(VideosPage));
        Routing.RegisterRoute(nameof(PlaylistsPage), typeof(PlaylistsPage));
        Routing.RegisterRoute(nameof(FoldersPage), typeof(FoldersPage));
        Routing.RegisterRoute(nameof(ArtistsPage), typeof(ArtistsPage));
        Routing.RegisterRoute(nameof(AlbumsPage), typeof(AlbumsPage));

        // Player tools
        Routing.RegisterRoute(nameof(QueuePage), typeof(QueuePage));
        Routing.RegisterRoute(nameof(LyricsPage), typeof(LyricsPage));
        Routing.RegisterRoute(nameof(EqualizerPage), typeof(EqualizerPage));

        // Settings sub-pages
        Routing.RegisterRoute(nameof(StatisticsPage), typeof(StatisticsPage));
    }
}
