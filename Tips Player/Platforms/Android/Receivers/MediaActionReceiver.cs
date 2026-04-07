using Android.Content;

namespace Tips_Player.Platforms.Android.Receivers;

[BroadcastReceiver(
    Name     = "gh.websitedesignerghana.tipsplayer.MediaActionReceiver",
    Enabled  = true,
    Exported = false)]
public class MediaActionReceiver : BroadcastReceiver
{
    public const string ActionPlayPause = "gh.websitedesignerghana.tipsplayer.PLAY_PAUSE";
    public const string ActionNext      = "gh.websitedesignerghana.tipsplayer.NEXT";
    public const string ActionPrev      = "gh.websitedesignerghana.tipsplayer.PREV";
    public const string ActionStop      = "gh.websitedesignerghana.tipsplayer.STOP";

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
