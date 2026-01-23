using Tips_Player.ViewModels;

namespace Tips_Player.Views;

public partial class FoldersPage : ContentPage
{
    public FoldersPage(FoldersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
