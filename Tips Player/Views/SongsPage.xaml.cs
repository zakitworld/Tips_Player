using Tips_Player.ViewModels;

namespace Tips_Player.Views;

public partial class SongsPage : ContentPage
{
    public SongsPage(SongsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
