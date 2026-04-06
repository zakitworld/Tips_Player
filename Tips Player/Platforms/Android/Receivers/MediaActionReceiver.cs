using Android.Content;

namespace Tips_Player.Platforms.Android.Receivers;

[BroadcastReceiver(
    Name     = "com.companyname.tipsplayer.MediaActionReceiver",
    Enabled  = true,
    Exported = false)]
public class MediaActionReceiver : BroadcastReceiver
{
    public const string ActionPlayPause = "com.companyname.tipsplayer.PLAY_PAUSE";
    public const string ActionNext      = "com.companyname.tipsplayer.NEXT";
    public const string ActionPrev      = "com.companyname.tipsplayer.PREV";
    public const string ActionStop      = "com.companyname.tipsplayer.STOP";

    public override void OnReceive(Context? context, Intent? intent)
    {
        switch (intent?.Action)
        {
            case ActionPlayPause: MediaServiceBridge.TriggerPlayPause(); break;
            case ActionNext:      MediaServiceBridge.TriggerNext();      break;
            case ActionPrev:      MediaServiceBridge.TriggerPrevious();  break;
            case ActionStop:      MediaServiceBridge.TriggerStop();      break;
        }
    }
}
