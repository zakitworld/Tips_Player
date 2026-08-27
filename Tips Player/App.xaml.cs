using Microsoft.Extensions.DependencyInjection;
using Tips_Player.ViewModels;

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using System.Runtime.InteropServices;
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
            AppShell shell;
            try
            {
                shell = new AppShell();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TipsPlayer] FATAL: AppShell init failed: {ex}");
                throw new InvalidOperationException($"AppShell init failed: {ex.Message}", ex);
            }

            var window = new Window(shell);

#if WINDOWS
            window.HandlerChanged += (s, e) =>
            {
                var viewModel = window.Handler?.MauiContext?.Services.GetService<PlayerViewModel>();
                if (viewModel != null)
                {
                    viewModel.PropertyChanged += (sender, args) =>
                    {
                        if (args.PropertyName == nameof(PlayerViewModel.IsFullScreen))
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                                UpdateWindowsFullscreen(window, viewModel.IsFullScreen));
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
            try
            {
                var nativeWindow = window.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
                if (nativeWindow == null) return;

                var windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                if (windowHandle == IntPtr.Zero) return;

                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                if (appWindow == null) return;

                var requestedKind = isFullScreen
                    ? AppWindowPresenterKind.FullScreen
                    : AppWindowPresenterKind.Default;
                if (appWindow.Presenter.Kind != requestedKind)
                    appWindow.SetPresenter(requestedKind);
            }
            catch (COMException ex)
            {
                // Some Windows configurations reject presenter transitions while a
                // modal page/layout transition is in progress. The modal video page
                // still fills the client area, so this must never terminate playback.
                System.Diagnostics.Debug.WriteLine($"[TipsPlayer] Fullscreen presenter unavailable: {ex}");
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TipsPlayer] Fullscreen transition unavailable: {ex}");
            }
        }
#endif
    }
}
