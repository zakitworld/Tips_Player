using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

namespace Tips_Player
{
    [Activity(
        Name                = "com.companyname.tipsplayer.MainActivity",
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

        public override void OnRequestPermissionsResult(
            int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
