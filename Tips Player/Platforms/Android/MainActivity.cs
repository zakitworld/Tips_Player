using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Tips_Player.ViewModels;

namespace Tips_Player
{
    [Activity(
        Name                = "gh.websitedesignerghana.tipsplayer.MainActivity",
        Theme               = "@style/Maui.SplashTheme",
        MainLauncher        = true,
        LaunchMode          = LaunchMode.SingleTop,
        ConfigurationChanges =
            ConfigChanges.ScreenSize     | ConfigChanges.Orientation     |
            ConfigChanges.UiMode         | ConfigChanges.ScreenLayout     |
            ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static MainActivity? Instance { get; private set; }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            Instance = this;
            base.OnCreate(savedInstanceState);
        }

        protected override void OnDestroy()
        {
            if (Instance == this) Instance = null;
            base.OnDestroy();
        }

        protected override void OnStop()
        {
            base.OnStop();
            Window?.ClearFlags(Android.Views.WindowManagerFlags.KeepScreenOn);

            var viewModel = IPlatformApplication.Current?.Services.GetService<PlayerViewModel>();
            if (viewModel != null)
                MainThread.BeginInvokeOnMainThread(async () => await viewModel.PauseVideoForBackgroundAsync());
        }

        public static void SetKeepScreenOn(bool enabled)
        {
            var activity = Instance;
            if (activity?.Window == null) return;
            activity.RunOnUiThread(() =>
            {
                if (enabled)
                    activity.Window.AddFlags(Android.Views.WindowManagerFlags.KeepScreenOn);
                else
                    activity.Window.ClearFlags(Android.Views.WindowManagerFlags.KeepScreenOn);
            });
        }

        public override void OnRequestPermissionsResult(
            int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
#pragma warning disable CA1416  // base method requires API 23+; MAUI ensures this is only called on 23+
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
#pragma warning restore CA1416
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
