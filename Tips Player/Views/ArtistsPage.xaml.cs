using Tips_Player.ViewModels;

namespace Tips_Player.Views;

public partial class ArtistsPage : ContentPage
{
    public ArtistsPage(ArtistsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
