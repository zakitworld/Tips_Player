using Tips_Player.ViewModels;

namespace Tips_Player.Views;

public partial class LibraryPage : ContentPage
{
    private LibraryViewModel? _viewModel;

    public LibraryPage()
    {
        InitializeComponent();
    }

    // Constructor for DI (when resolved through service provider)
    public LibraryPage(LibraryViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // If ViewModel wasn't injected, get it from the service provider
        if (_viewModel == null)
        {
            var libraryVm = Handler?.MauiContext?.Services.GetService<LibraryViewModel>();

            if (libraryVm != null)
            {
                _viewModel = libraryVm;
                BindingContext = libraryVm;
            }
        }

        // Refresh stats when page appears
        _viewModel?.RefreshStats();
    }
}
