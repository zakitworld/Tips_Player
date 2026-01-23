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
    }

    private void OnSliderDragCompleted(object? sender, EventArgs e)
    {
        _viewModel?.OnSliderDragCompleted();
    }

    private void OnSliderValueChanged(object? sender, ValueChangedEventArgs e)
    {
        _viewModel?.OnSliderValueChanged(e.NewValue);
    }
}
