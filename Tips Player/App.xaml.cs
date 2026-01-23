using Microsoft.Extensions.DependencyInjection;
using Tips_Player.ViewModels;

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
#endif

namespace Tips_Player
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

#if WINDOWS
            window.HandlerChanged += (s, e) =>
            {
                var viewModel = Handler.MauiContext.Services.GetService<PlayerViewModel>();
                if (viewModel != null)
                {
                    viewModel.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(PlayerViewModel.IsFullScreen))
                        {
                            UpdateWindowsFullscreen(window, viewModel.IsFullScreen);
                        }
                    };
                }
            };
#endif
            return window;
        }

#if WINDOWS
        private void UpdateWindowsFullscreen(Window window, bool isFullScreen)
        {
            var nativeWindow = window.Handler.PlatformView as Microsoft.UI.Xaml.Window;
            if (nativeWindow == null) return;

            var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            if (appWindow != null)
            {
                if (isFullScreen)
                {
                    appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                }
                else
                {
                    appWindow.SetPresenter(AppWindowPresenterKind.Default);
                }
            }
        }
#endif
    }
}