using Tips_Player.ViewModels;

namespace Tips_Player.Views;

public partial class LibraryPage : ContentPage
{
    private LibraryViewModel? _viewModel;
    private PlayerViewModel? _playerViewModel;

    public LibraryPage()
    {
        InitializeComponent();
    }

    // Constructor for DI (when resolved through service provider)
    public LibraryPage(LibraryViewModel viewModel, PlayerViewModel playerViewModel) : this()
    {
        SetupViewModels(viewModel, playerViewModel);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // If ViewModels weren't injected, get them from the service provider
        if (_viewModel == null || _playerViewModel == null)
        {
            var libraryVm = Handler?.MauiContext?.Services.GetService<LibraryViewModel>();
            var playerVm = Handler?.MauiContext?.Services.GetService<PlayerViewModel>();

            if (libraryVm != null && playerVm != null)
            {
                SetupViewModels(libraryVm, playerVm);
            }
        }

        // Update mini player binding
        if (_playerViewModel != null && MiniPlayerBar != null)
        {
            MiniPlayerBar.BindingContext = _playerViewModel;
        }
    }

    private void SetupViewModels(LibraryViewModel viewModel, PlayerViewModel playerViewModel)
    {
        _viewModel = viewModel;
        _playerViewModel = playerViewModel;
        BindingContext = viewModel;
    }
}
