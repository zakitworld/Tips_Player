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
                // Fullscreen: Make MediaElement fill the entire screen
                MediaElement.HorizontalOptions = LayoutOptions.Fill;
                MediaElement.VerticalOptions = LayoutOptions.Fill;
                MediaElement.Margin = new Thickness(0); // Fill entire window
                MediaElement.HeightRequest = -1; // Auto
                MediaElement.ZIndex = 11;
            }
            else
            {
                // Normal: Position MediaElement within the border container
                bool isDesktop = Width > 850;
                MediaElement.HorizontalOptions = LayoutOptions.Fill;
                MediaElement.ZIndex = 11;

                if (isDesktop)
                {
                    // Desktop: Media on left, controls on right (400 width)
                    // Margin: Header (72) + side padding (24)
                    var newMargin = new Thickness(24, 72, 424, 24);
                    if (MediaElement.VerticalOptions != LayoutOptions.Fill ||
                        MediaElement.Margin != newMargin ||
                        MediaElement.HeightRequest != -1)
                    {
                        MediaElement.VerticalOptions = LayoutOptions.Fill;
                        MediaElement.Margin = newMargin;
                        MediaElement.HeightRequest = -1;
                    }
                }
                else
                {
                    // Mobile: Vertical stack
                    var newMargin = new Thickness(24, 72, 24, 0);
                    if (MediaElement.VerticalOptions != LayoutOptions.Start ||
                        MediaElement.Margin != newMargin ||
                        MediaElement.HeightRequest != 320)
                    {
                        MediaElement.VerticalOptions = LayoutOptions.Start;
                        MediaElement.Margin = newMargin;
                        MediaElement.HeightRequest = 320;
                    }
                }
            }
            _isUpdatingLayout = false;
        });
    }

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
}
