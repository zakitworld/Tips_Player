using Tips_Player.ViewModels;

namespace Tips_Player.Views;

public partial class VideosPage : ContentPage
{
    public VideosPage(VideosViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
