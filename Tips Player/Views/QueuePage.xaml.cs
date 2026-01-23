using Tips_Player.ViewModels;

namespace Tips_Player.Views;

public partial class QueuePage : ContentPage
{
    public QueuePage(QueueViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
