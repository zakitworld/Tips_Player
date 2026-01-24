using Tips_Player.ViewModels;

namespace Tips_Player.Views;

public partial class LyricsPage : ContentPage
{
    private readonly LyricsViewModel _viewModel;

    public LyricsPage(LyricsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnDisappearing();
    }
}
