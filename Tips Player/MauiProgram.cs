using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Serilog;
using Tips_Player.Infrastructure.Logging;
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

        Step("UseMauiApp", () => builder.UseMauiApp<App>());
        Step("UseMauiCommunityToolkit", () => builder.UseMauiCommunityToolkit());
        Step("UseMauiCommunityToolkitMediaElement", () => builder.UseMauiCommunityToolkitMediaElement());
        Step("ConfigureSerilog", () => builder.ConfigureSerilog());
        Step("ConfigureFonts", () => builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            fonts.AddFont("fa-solid-900.ttf", "FontAwesome");
        }));

        // Register Services
        Step("RegisterServices", () =>
        {
            builder.Services.AddSingleton<IMediaPlayerService, MediaPlayerService>();
            builder.Services.AddSingleton<IFilePickerService, FilePickerService>();
            builder.Services.AddSingleton<ILibraryService, LibraryService>();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<ILyricsService, LyricsService>();
            builder.Services.AddSingleton<IStatisticsService, StatisticsService>();

#if ANDROID
            // Platform-specific services
            builder.Services.AddSingleton<IMediaScannerService,
                Tips_Player.Platforms.Android.Services.MediaScannerService>();
            builder.Services.AddSingleton<IAlbumArtService,
                Tips_Player.Platforms.Android.Services.AlbumArtService>();
            // Android hardware equalizer (inherits EqualizerService for persistence)
            builder.Services.AddSingleton<Tips_Player.Platforms.Android.Services.AndroidEqualizerService>();
            builder.Services.AddSingleton<IEqualizerService>(sp =>
                sp.GetRequiredService<Tips_Player.Platforms.Android.Services.AndroidEqualizerService>());
#else
            builder.Services.AddSingleton<IMediaScannerService, NullMediaScannerService>();
            builder.Services.AddSingleton<IAlbumArtService, NullAlbumArtService>();
            builder.Services.AddSingleton<IEqualizerService, EqualizerService>();
#endif
        });

        // Register ViewModels
        Step("RegisterViewModels", () =>
        {
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
        });

        // Register Pages
        Step("RegisterPages", () =>
        {
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
        });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return Step("Build", () => builder.Build());
    }

    /// <summary>
    /// Wraps each startup step so any exception is logged with the step name
    /// before it propagates — turning a generic JavaProxyThrowable into a
    /// specific, actionable error message in logcat and crash.txt.
    /// </summary>
    private static void Step(string name, Action action)
    {
        try
        {
            action();
            Log.Debug("TipsPlayer.Startup", "✓ {Step}", name);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "✗ Startup step '{Step}' failed", name);
            global::System.Diagnostics.Debug.WriteLine($"[TipsPlayer] FATAL: step '{name}' failed: {ex}");
            throw new InvalidOperationException($"Startup step '{name}' failed: {ex.Message}", ex);
        }
    }

    private static T Step<T>(string name, Func<T> func)
    {
        try
        {
            var result = func();
            Log.Debug("TipsPlayer.Startup", "✓ {Step}", name);
            return result;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "✗ Startup step '{Step}' failed", name);
            global::System.Diagnostics.Debug.WriteLine($"[TipsPlayer] FATAL: step '{name}' failed: {ex}");
            throw new InvalidOperationException($"Startup step '{name}' failed: {ex.Message}", ex);
        }
    }
}
