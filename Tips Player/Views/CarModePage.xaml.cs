using Tips_Player.ViewModels;

namespace Tips_Player.Views;

public partial class CarModePage : ContentPage
{
    public CarModePage(CarModeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Keep screen awake during car mode
        DeviceDisplay.Current.KeepScreenOn = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Allow screen to sleep again
        DeviceDisplay.Current.KeepScreenOn = false;
    }
}
