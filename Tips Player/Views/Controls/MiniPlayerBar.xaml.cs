namespace Tips_Player.Views.Controls;

public partial class MiniPlayerBar : ContentView
{
    public MiniPlayerBar()
    {
        InitializeComponent();
    }

    private async void OnMiniPlayerTapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//PlayerPage");
    }
}
