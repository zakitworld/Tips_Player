using CommunityToolkit.Maui.Core;
using Tips_Player.ViewModels;

namespace Tips_Player.Views;

public partial class PlayerPage : ContentPage
{
    private PlayerViewModel? _viewModel;
    private bool _enteringFullscreen;

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
#if ANDROID
        if (e.PropertyName is nameof(PlayerViewModel.ShowVideoPlayer) or nameof(PlayerViewModel.IsPlaying))
            Tips_Player.MainActivity.SetKeepScreenOn(
                _viewModel?.ShowVideoPlayer == true && _viewModel.IsPlaying);
#endif
    }

    private async Task EnterFullscreenAsync()
    {
        if (_viewModel == null || _enteringFullscreen) return;
        _enteringFullscreen = true;

        try
        {
            var position  = MediaElement.Position;
            var isPlaying = MediaElement.CurrentState == MediaElementState.Playing;

            // Pause so there's no audio overlap during the modal transition.
            MediaElement.Pause();

            var fullscreenPage = new FullscreenVideoPage(
                _viewModel, MediaElement, position, isPlaying);

            await Navigation.PushModalAsync(fullscreenPage, animated: false);
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or InvalidOperationException)
        {
            _viewModel.IsFullScreen = false;
            _viewModel.SetMediaElement(MediaElement);
            System.Diagnostics.Debug.WriteLine($"[TipsPlayer] Could not enter fullscreen: {ex}");
            await DisplayAlertAsync("Fullscreen unavailable",
                "Windows could not switch this video to fullscreen. Playback can continue in the player window.",
                "OK");
        }
        finally
        {
            _enteringFullscreen = false;
        }
    }

    private void UpdateVideoControlsVisibility()
    {
        if (_viewModel == null) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            bool show = _viewModel.ShowVideoPlayer && !_viewModel.IsFullScreen;
            NormalVideoControls.IsVisible = show;
        });
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        UpdateVideoControlsVisibility();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
#if ANDROID
        Tips_Player.MainActivity.SetKeepScreenOn(false);
#endif
        if (_viewModel?.ShowVideoPlayer == true && !_viewModel.IsFullScreen)
            _ = _viewModel.PauseVideoForBackgroundAsync();
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
