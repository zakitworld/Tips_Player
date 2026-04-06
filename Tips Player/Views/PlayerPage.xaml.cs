using CommunityToolkit.Maui.Core.Primitives;
using Tips_Player.ViewModels;

namespace Tips_Player.Views;

public partial class PlayerPage : ContentPage
{
    private PlayerViewModel? _viewModel;

    public PlayerPage()
    {
        InitializeComponent();
    }

    // Constructor for DI (when resolved through service provider)
    public PlayerPage(PlayerViewModel viewModel) : this()
    {
        SetupViewModel(viewModel);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // If ViewModel wasn't injected, get it from the service provider
        if (_viewModel == null)
        {
            var viewModel = Handler?.MauiContext?.Services.GetService<PlayerViewModel>();
            if (viewModel != null)
            {
                SetupViewModel(viewModel);
            }
        }

        if (_viewModel != null)
        {
            await _viewModel.InitializeAsync();
            _viewModel.SetMediaElement(MediaElement);
            UpdateMediaElementLayout();
            UpdateVideoControlsVisibility();
        }
    }

    private void SetupViewModel(PlayerViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        // Re-position the floating MediaElement whenever the container is laid out
        NormalMediaContainer.SizeChanged += (_, _) => UpdateMediaElementLayout();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlayerViewModel.IsFullScreen))
        {
            UpdateMediaElementLayout();
            UpdateVideoControlsVisibility();
        }
        else if (e.PropertyName == nameof(PlayerViewModel.ShowVideoPlayer))
        {
            UpdateVideoControlsVisibility();
        }
    }

    private void UpdateVideoControlsVisibility()
    {
        if (_viewModel == null) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Show video controls only when playing video AND not in fullscreen
            bool shouldShow = _viewModel.ShowVideoPlayer && !_viewModel.IsFullScreen;
            NormalVideoControls.IsVisible = shouldShow;

            if (shouldShow)
            {
                UpdateVideoControlsPosition();
            }
        });
    }

    private void UpdateVideoControlsPosition()
    {
        bool isDesktop = Width > 850;
        if (isDesktop)
        {
            // Desktop: Position at top-right of video area (video ends at Width - 424)
            // Video area: left=24, top=72, right=Width-424
            // Controls at top-right of video: right margin = 424 + 12 (padding)
            NormalVideoControls.HorizontalOptions = LayoutOptions.End;
            NormalVideoControls.Margin = new Thickness(0, 84, 436, 0);
        }
        else
        {
            // Mobile: Position at top-right of video area
            NormalVideoControls.HorizontalOptions = LayoutOptions.End;
            NormalVideoControls.Margin = new Thickness(0, 84, 36, 0);
        }
    }

    private bool _isUpdatingLayout;
    private void UpdateMediaElementLayout()
    {
        if (_viewModel == null || _isUpdatingLayout || Width <= 0) return;
        _isUpdatingLayout = true;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_viewModel.IsFullScreen)
            {
                // Fullscreen: MediaElement fills FullscreenContainer.
                // No ZIndex juggling needed — MediaElement is the first child of
                // FullscreenContainer so ExoPlayer SurfaceView renders below the controls.
                FullscreenContainer.InputTransparent = false;
                FullscreenContainer.BackgroundColor  = Colors.Black;

                MediaElement.HorizontalOptions = LayoutOptions.Fill;
                MediaElement.VerticalOptions   = LayoutOptions.Fill;
                MediaElement.Margin            = new Thickness(0);
                MediaElement.HeightRequest     = -1;
                MediaElement.WidthRequest      = -1;

#if ANDROID
                SetAndroidImmersiveMode(true);
#endif
            }
            else
            {
#if ANDROID
                SetAndroidImmersiveMode(false);
#endif
                // Normal mode: FullscreenContainer is transparent + input-passthrough.
                FullscreenContainer.InputTransparent = true;
                FullscreenContainer.BackgroundColor  = Colors.Transparent;

                // Overlay MediaElement exactly over NormalMediaContainer.
                // Bounds gives X/Y relative to the page automatically.
                var bounds = NormalMediaContainer.Bounds;
                if (bounds.Width > 0 && bounds.Height > 0)
                {
                    MediaElement.HorizontalOptions = LayoutOptions.Start;
                    MediaElement.VerticalOptions   = LayoutOptions.Start;
                    MediaElement.Margin            = new Thickness(bounds.X, bounds.Y, 0, 0);
                    MediaElement.WidthRequest      = bounds.Width;
                    MediaElement.HeightRequest     = bounds.Height;
                }
                else
                {
                    // Fallback until first layout pass completes
                    bool isDesktop = Width > 850;
                    MediaElement.HorizontalOptions = LayoutOptions.Fill;
                    if (isDesktop)
                    {
                        MediaElement.VerticalOptions = LayoutOptions.Fill;
                        MediaElement.Margin          = new Thickness(24, 72, 424, 24);
                        MediaElement.HeightRequest   = -1;
                        MediaElement.WidthRequest    = -1;
                    }
                    else
                    {
                        MediaElement.VerticalOptions = LayoutOptions.Start;
                        MediaElement.Margin          = new Thickness(24, 56, 24, 0);
                        MediaElement.HeightRequest   = 280;
                        MediaElement.WidthRequest    = -1;
                    }
                }
            }
            _isUpdatingLayout = false;
        });
    }

#if ANDROID
    private static void SetAndroidImmersiveMode(bool enter)
    {
        var window = Platform.CurrentActivity?.Window;
        if (window == null) return;

#pragma warning disable CA1416
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
#pragma warning restore CA1416
    }
#endif

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        UpdateMediaElementLayout();
        UpdateVideoControlsVisibility();
    }

    private void OnMediaOpened(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_viewModel != null && MediaElement.Duration != TimeSpan.Zero)
            {
                _viewModel.UpdateDuration(MediaElement.Duration);
            }
        });
    }

    private void OnPositionChanged(object? sender, MediaPositionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_viewModel != null && _viewModel.Duration == TimeSpan.Zero && MediaElement.Duration != TimeSpan.Zero)
            {
                _viewModel.UpdateDuration(MediaElement.Duration);
            }
        });
    }

    private void OnSliderDragStarted(object? sender, EventArgs e)
    {
        _viewModel?.OnSliderDragStarted();
        // Keep controls visible while seeking
        if (_viewModel?.IsFullScreen == true)
        {
            _viewModel?.ShowFullscreenControls();
        }
    }

    private void OnSliderDragCompleted(object? sender, EventArgs e)
    {
        _viewModel?.OnSliderDragCompleted();
        // Reset auto-hide timer after seeking
        if (_viewModel?.IsFullScreen == true)
        {
            _viewModel?.ShowFullscreenControls();
        }
    }

    private void OnSliderValueChanged(object? sender, ValueChangedEventArgs e)
    {
        _viewModel?.OnSliderValueChanged(e.NewValue);
    }

    // Called when user taps on fullscreen video area to show controls
    private void OnFullscreenVideoTapped(object? sender, TappedEventArgs e)
    {
        _viewModel?.ShowFullscreenControls();
    }

    // Swipe left → next track, swipe right → previous track
    private double _panX;

    private async void OnAlbumArtPanned(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Running:
                _panX = e.TotalX;
                break;

            case GestureStatus.Completed:
                if (_viewModel == null) break;
                if (_panX < -60 && _viewModel.HasNext)
                    await _viewModel.NextCommand.ExecuteAsync(null);
                else if (_panX > 60 && _viewModel.HasPrevious)
                    await _viewModel.PreviousCommand.ExecuteAsync(null);
                _panX = 0;
                break;
        }
    }
}
