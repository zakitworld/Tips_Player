using Tips_Player.ViewModels;

namespace Tips_Player.Views;

public partial class AlbumsPage : ContentPage
{
    public AlbumsPage(AlbumsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
