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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
        await _viewModel.LoadLyricsAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnDisappearing();
    }
}
