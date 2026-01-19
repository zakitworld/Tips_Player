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
            });

        // Register Services
        builder.Services.AddSingleton<IMediaPlayerService, MediaPlayerService>();
        builder.Services.AddSingleton<IFilePickerService, FilePickerService>();

        // Register ViewModels
        builder.Services.AddSingleton<PlayerViewModel>();
        builder.Services.AddSingleton<LibraryViewModel>();

        // Register Pages
        builder.Services.AddSingleton<PlayerPage>();
        builder.Services.AddSingleton<LibraryPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
