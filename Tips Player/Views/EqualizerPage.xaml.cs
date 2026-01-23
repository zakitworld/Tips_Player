using Tips_Player.ViewModels;

namespace Tips_Player.Views;

public partial class EqualizerPage : ContentPage
{
    public EqualizerPage(EqualizerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
