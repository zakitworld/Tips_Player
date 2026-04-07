using CommunityToolkit.Maui.Core.Primitives;
using Tips_Player.ViewModels;

namespace Tips_Player.Views;

public partial class FullscreenVideoPage : ContentPage
{
    private readonly PlayerViewModel _viewModel;
    private readonly CommunityToolkit.Maui.Views.MediaElement _originalElement;
    private TimeSpan _resumePosition;
    private bool _wasPlaying;
    private bool _dismissing;

    // Auto-hide controls after 3 s of inactivity
    private System.Timers.Timer? _hideTimer;

    public FullscreenVideoPage(
        PlayerViewModel viewModel,
        CommunityToolkit.Maui.Views.MediaElement originalElement,
        TimeSpan position,
        bool wasPlaying)
    {
        InitializeComponent();
        _viewModel      = viewModel;
        _originalElement = originalElement;
        _resumePosition = position;
        _wasPlaying     = wasPlaying;
        BindingContext  = viewModel;
    }

    // ───────── lifecycle ─────────

    protected override async void OnAppearing()
    {
        base.OnAppearing();

#if ANDROID
        SetImmersiveMode(true);
#endif
        ResetHideTimer();

        // Hand the active MediaElement to the player service
        _viewModel.SetMediaElement(VideoElement);

        // Reload same media from the saved position
        await _viewModel.ResumeAtPositionAsync(_resumePosition, _wasPlaying);
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        if (_dismissing) return;
        _dismissing = true;

        _hideTimer?.Dispose();
        _hideTimer = null;

        // Save current playback state before handing back
        var pos     = VideoElement.Position;
        var playing = VideoElement.CurrentState == MediaElementState.Playing;

        VideoElement.Stop();

        // Return control to PlayerPage's MediaElement
        _viewModel.SetMediaElement(_originalElement);
        await _viewModel.ResumeAtPositionAsync(pos, playing);

#if ANDROID
        SetImmersiveMode(false);
#endif
    }

    // ───────── event handlers ─────────

    private void OnBackClicked(object? sender, EventArgs e)
    {
        ExitFullscreen();
    }

    private void OnScreenTapped(object? sender, TappedEventArgs e)
    {
        ControlsOverlay.IsVisible = !ControlsOverlay.IsVisible;
        if (ControlsOverlay.IsVisible)
            ResetHideTimer();
        else
            _hideTimer?.Stop();
    }

    private void OnScreenDoubleTapped(object? sender, TappedEventArgs e)
    {
        ExitFullscreen();
    }

    private void OnAspectToggled(object? sender, EventArgs e)
    {
        _viewModel.VideoAspect = _viewModel.VideoAspect == Aspect.AspectFit
            ? Aspect.AspectFill
            : Aspect.AspectFit;
        ResetHideTimer();
    }

    private void OnMediaOpened(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (VideoElement.Duration != TimeSpan.Zero)
                _viewModel.UpdateDuration(VideoElement.Duration);
        });
    }

    private void OnPositionChanged(object? sender,
        CommunityToolkit.Maui.Core.Primitives.MediaPositionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_viewModel.Duration == TimeSpan.Zero && VideoElement.Duration != TimeSpan.Zero)
                _viewModel.UpdateDuration(VideoElement.Duration);
        });
    }

    private void OnSliderDragStarted(object? sender, EventArgs e)
    {
        _viewModel.OnSliderDragStarted();
        ResetHideTimer();
    }

    private void OnSliderDragCompleted(object? sender, EventArgs e)
    {
        _viewModel.OnSliderDragCompleted();
        ResetHideTimer();
    }

    // ───────── helpers ─────────

    private void ExitFullscreen()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _viewModel.IsFullScreen = false;
            await Navigation.PopModalAsync(animated: false);
        });
    }

    private void ResetHideTimer()
    {
        _hideTimer?.Stop();
        _hideTimer?.Dispose();
        _hideTimer = new System.Timers.Timer(3000);
        _hideTimer.AutoReset = false;
        _hideTimer.Elapsed += (_, _) =>
            MainThread.BeginInvokeOnMainThread(() => ControlsOverlay.IsVisible = false);
        _hideTimer.Start();
    }

#if ANDROID
    private static void SetImmersiveMode(bool enter)
    {
        var window = Platform.CurrentActivity?.Window;
        if (window == null) return;

#pragma warning disable CA1416, CA1422  // SystemUiFlags obsolete on API 30+ but still functional; runtime-guarded
        if (enter)
        {
            window.AddFlags(Android.Views.WindowManagerFlags.Fullscreen);
            window.DecorView.SystemUiFlags =
                Android.Views.SystemUiFlags.ImmersiveSticky |
                Android.Views.SystemUiFlags.HideNavigation |
                Android.Views.SystemUiFlags.Fullscreen |
                Android.Views.SystemUiFlags.LayoutHideNavigation |
                Android.Views.SystemUiFlags.LayoutFullscreen |
                Android.Views.SystemUiFlags.LayoutStable;
        }
        else
        {
            window.ClearFlags(Android.Views.WindowManagerFlags.Fullscreen);
            window.DecorView.SystemUiFlags = Android.Views.SystemUiFlags.Visible;
        }
#pragma warning restore CA1416, CA1422
    }
#endif
}
