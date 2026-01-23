using Tips_Player.ViewModels;

namespace Tips_Player.Views;

public partial class PlaylistsPage : ContentPage
{
    public PlaylistsPage(PlaylistsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
