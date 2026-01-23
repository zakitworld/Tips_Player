using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Tips_Player.Services;
using Tips_Player.Services.Interfaces;
using Tips_Player.ViewModels;
using Tips_Player.Views;

namespace Tips_Player;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("fa-solid-900.ttf", "FontAwesome");
            });

        // Register Services
        builder.Services.AddSingleton<IMediaPlayerService, MediaPlayerService>();
        builder.Services.AddSingleton<IFilePickerService, FilePickerService>();
        builder.Services.AddSingleton<ILibraryService, LibraryService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IEqualizerService, EqualizerService>();
        builder.Services.AddSingleton<ILyricsService, LyricsService>();
        builder.Services.AddSingleton<IStatisticsService, StatisticsService>();

        // Register ViewModels
        builder.Services.AddSingleton<PlayerViewModel>();
        builder.Services.AddSingleton<LibraryViewModel>();
        builder.Services.AddSingleton<SongsViewModel>();
        builder.Services.AddSingleton<VideosViewModel>();
        builder.Services.AddSingleton<PlaylistsViewModel>();
        builder.Services.AddSingleton<FoldersViewModel>();
        builder.Services.AddSingleton<ArtistsViewModel>();
        builder.Services.AddSingleton<AlbumsViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();
        builder.Services.AddSingleton<EqualizerViewModel>();
        builder.Services.AddSingleton<QueueViewModel>();
        builder.Services.AddSingleton<SearchViewModel>();
        builder.Services.AddSingleton<StatisticsViewModel>();
        builder.Services.AddSingleton<LyricsViewModel>();
        builder.Services.AddSingleton<CarModeViewModel>();

        // Register Pages
        builder.Services.AddSingleton<PlayerPage>();
        builder.Services.AddSingleton<LibraryPage>();
        builder.Services.AddSingleton<SongsPage>();
        builder.Services.AddSingleton<VideosPage>();
        builder.Services.AddSingleton<PlaylistsPage>();
        builder.Services.AddSingleton<FoldersPage>();
        builder.Services.AddSingleton<ArtistsPage>();
        builder.Services.AddSingleton<AlbumsPage>();
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<EqualizerPage>();
        builder.Services.AddSingleton<QueuePage>();
        builder.Services.AddSingleton<SearchPage>();
        builder.Services.AddSingleton<StatisticsPage>();
        builder.Services.AddSingleton<LyricsPage>();
        builder.Services.AddTransient<CarModePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
