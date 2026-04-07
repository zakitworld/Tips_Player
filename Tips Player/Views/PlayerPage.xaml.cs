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

    public PlayerPage(PlayerViewModel viewModel) : this()
    {
        SetupViewModel(viewModel);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel == null)
        {
            var viewModel = Handler?.MauiContext?.Services.GetService<PlayerViewModel>();
            if (viewModel != null)
                SetupViewModel(viewModel);
        }

        if (_viewModel != null)
        {
            await _viewModel.InitializeAsync();

            // Always (re)claim the MediaElement when this page appears — including
            // when returning from the fullscreen modal.
            _viewModel.SetMediaElement(MediaElement);

            UpdateMediaElementLayout();
            UpdateVideoControlsVisibility();
        }
    }

    private void SetupViewModel(PlayerViewModel viewModel)
    {
        _viewModel     = viewModel;
        BindingContext = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        NormalMediaContainer.SizeChanged += (_, _) =>
        {
            UpdateMediaElementLayout();
            UpdateVideoControlsVisibility();
        };
    }

    private void OnViewModelPropertyChanged(object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PlayerViewModel.IsFullScreen) &&
            _viewModel?.IsFullScreen == true)
        {
            // Launch fullscreen as a modal page — the only reliable way to get
            // true full-screen video on Android without fighting ExoPlayer.
            MainThread.BeginInvokeOnMainThread(async () => await EnterFullscreenAsync());
        }
        else if (e.PropertyName == nameof(PlayerViewModel.ShowVideoPlayer))
        {
            UpdateVideoControlsVisibility();
        }
    }

    private async Task EnterFullscreenAsync()
    {
        if (_viewModel == null) return;

        var position  = MediaElement.Position;
        var isPlaying = MediaElement.CurrentState == MediaElementState.Playing;

        // Pause so there's no audio overlap during the modal transition
        MediaElement.Pause();

        var fullscreenPage = new FullscreenVideoPage(
            _viewModel, MediaElement, position, isPlaying);

        await Navigation.PushModalAsync(fullscreenPage, animated: false);
    }

    // ───────── normal-mode video overlay positioning ─────────

    private bool _isUpdatingLayout;
    private void UpdateMediaElementLayout()
    {
        if (_viewModel == null || _isUpdatingLayout || Width <= 0) return;
        _isUpdatingLayout = true;

        MainThread.BeginInvokeOnMainThread(() =>
        {
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
                // Fallback until first layout pass provides bounds
                MediaElement.HorizontalOptions = LayoutOptions.Fill;
                MediaElement.VerticalOptions   = LayoutOptions.Start;
                MediaElement.Margin = Width > 850
                    ? new Thickness(24, 72, 424, 0)
                    : new Thickness(24, 56, 24, 0);
                MediaElement.HeightRequest = Width > 850 ? -1 : 280;
                MediaElement.WidthRequest  = -1;
            }
            _isUpdatingLayout = false;
        });
    }

    private void UpdateVideoControlsVisibility()
    {
        if (_viewModel == null) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            bool show = _viewModel.ShowVideoPlayer && !_viewModel.IsFullScreen;
            NormalVideoControls.IsVisible = show;

            if (show)
            {
                var bounds    = NormalMediaContainer.Bounds;
                double right  = bounds.Width > 0
                    ? Width - (bounds.X + bounds.Width) + 8
                    : (Width > 850 ? 436 : 36);
                double top    = bounds.Width > 0 ? bounds.Y + 8 : 84;

                NormalVideoControls.HorizontalOptions = LayoutOptions.End;
                NormalVideoControls.VerticalOptions   = LayoutOptions.Start;
                NormalVideoControls.Margin            = new Thickness(0, top, right, 0);
            }
        });
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        UpdateMediaElementLayout();
        UpdateVideoControlsVisibility();
    }

    // ───────── event handlers ─────────

    private void OnMediaOpened(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_viewModel != null && MediaElement.Duration != TimeSpan.Zero)
                _viewModel.UpdateDuration(MediaElement.Duration);
        });
    }

    private void OnPositionChanged(object? sender, MediaPositionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_viewModel != null && _viewModel.Duration == TimeSpan.Zero &&
                MediaElement.Duration != TimeSpan.Zero)
                _viewModel.UpdateDuration(MediaElement.Duration);
        });
    }

    private void OnSliderDragStarted(object? sender, EventArgs e) =>
        _viewModel?.OnSliderDragStarted();

    private void OnSliderDragCompleted(object? sender, EventArgs e) =>
        _viewModel?.OnSliderDragCompleted();

    private void OnSliderValueChanged(object? sender, ValueChangedEventArgs e) =>
        _viewModel?.OnSliderValueChanged(e.NewValue);

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
