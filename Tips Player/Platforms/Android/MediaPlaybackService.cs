using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Tips_Player.Platforms.Android.Receivers;

// All code in this file runs only on Android. The version guards (Build.VERSION.SdkInt >=)
// already protect every API-gated call at runtime; pragmas just silence the analyzer.
#pragma warning disable CS8602, CS8604   // Null-dereference — Android builder chains are non-null by contract
#pragma warning disable CA1416           // Platform version — all calls are runtime-guarded
#pragma warning disable CA1422           // Obsolete API — kept for API 21-22 compatibility

namespace Tips_Player.Platforms.Android;

[Service(
    Name             = "gh.websitedesignerghana.tipsplayer.MediaPlaybackService",
    Exported         = false,
    ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeMediaPlayback)]
public class MediaPlaybackService : Service
{
    // ── Constants ─────────────────────────────────────────────────────────────
    private const int    NotificationId = 1001;
    private const string ChannelId      = "tips_player_playback";
    private const string ChannelName    = "Music Playback";

    // ── Singleton control ─────────────────────────────────────────────────────
    private static MediaPlaybackService? _instance;

    public static void Start()
    {
        var context = global::Android.App.Application.Context;
        var intent  = new Intent(context, typeof(MediaPlaybackService));
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            context.StartForegroundService(intent);
        else
            context.StartService(intent);
    }

    public static void Stop()
    {
        _instance?.StopSelf();
    }

    // ── Android Service lifecycle ─────────────────────────────────────────────
    private MediaSession? _mediaSession;

    public override IBinder? OnBind(Intent? intent) => null;

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        _instance = this;

        EnsureNotificationChannel();

        var notification = BuildNotification();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            StartForeground(NotificationId, notification,
                global::Android.Content.PM.ForegroundService.TypeMediaPlayback);
        else
            StartForeground(NotificationId, notification);

        InitMediaSession();

        MediaServiceBridge.StateChanged += OnBridgeStateChanged;

        return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
        MediaServiceBridge.StateChanged -= OnBridgeStateChanged;
        _mediaSession?.Release();
        _mediaSession = null;
        _instance     = null;
        base.OnDestroy();
    }

    // ── MediaSession ──────────────────────────────────────────────────────────

    private void InitMediaSession()
    {
        _mediaSession = new MediaSession(this, "TipsPlayerSession");
        _mediaSession.SetCallback(new MediaSessionCallback());
        _mediaSession.SetFlags(
            MediaSessionFlags.HandlesMediaButtons |
            MediaSessionFlags.HandlesTransportControls);
        _mediaSession.Active = true;
    }

    private void UpdateMediaSessionMetadata()
    {
        var media = MediaServiceBridge.CurrentMedia;
        if (_mediaSession == null || media == null) return;

        var builder = new MediaMetadata.Builder()
            .PutString(MediaMetadata.MetadataKeyTitle,  media.Title)!
            .PutString(MediaMetadata.MetadataKeyArtist, media.Artist)!
            .PutString(MediaMetadata.MetadataKeyAlbum,  media.Album)!
            .PutLong(MediaMetadata.MetadataKeyDuration, (long)media.Duration.TotalMilliseconds);

        if (!string.IsNullOrEmpty(media.AlbumArtPath) && File.Exists(media.AlbumArtPath))
        {
            try
            {
                var bmp = BitmapFactory.DecodeFile(media.AlbumArtPath);
                if (bmp != null) builder!.PutBitmap(MediaMetadata.MetadataKeyAlbumArt, bmp);
            }
            catch { /* non-fatal */ }
        }

        _mediaSession.SetMetadata(builder!.Build());

        var state = new PlaybackState.Builder()
            .SetActions(PlaybackState.ActionPlay | PlaybackState.ActionPause |
                        PlaybackState.ActionSkipToNext | PlaybackState.ActionSkipToPrevious |
                        PlaybackState.ActionStop)!
            .SetState(
                MediaServiceBridge.IsPlaying
                    ? PlaybackStateCode.Playing
                    : PlaybackStateCode.Paused,
                0, 1.0f)!
            .Build();
        _mediaSession.SetPlaybackState(state);
    }

    // ── Notification ──────────────────────────────────────────────────────────

    private void EnsureNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;

        var channel = new NotificationChannel(
            ChannelId, ChannelName, NotificationImportance.Low)
        {
            Description = "Shows current track and playback controls"
        };
        channel.SetShowBadge(false);
        var nm = (NotificationManager?)GetSystemService(NotificationService);
        nm?.CreateNotificationChannel(channel);
    }

    private Notification BuildNotification()
    {
        var media     = MediaServiceBridge.CurrentMedia;
        var isPlaying = MediaServiceBridge.IsPlaying;

        var context = global::Android.App.Application.Context;

        var tapIntent  = context.PackageManager!.GetLaunchIntentForPackage(context.PackageName!)!;
        var tapPending = PendingIntent.GetActivity(context, 0, tapIntent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;

        PendingIntent ActionIntent(string action, int reqCode)
        {
            var i = new Intent(action).SetPackage(context.PackageName);
            return PendingIntent.GetBroadcast(context, reqCode, i,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
        }

        var prevIntent = ActionIntent(MediaActionReceiver.ActionPrev,     10);
        var ppIntent   = ActionIntent(MediaActionReceiver.ActionPlayPause, 11);
        var nextIntent = ActionIntent(MediaActionReceiver.ActionNext,      12);
        var stopIntent = ActionIntent(MediaActionReceiver.ActionStop,      13);

        var playPauseIcon = isPlaying
            ? global::Android.Resource.Drawable.IcMediaPause
            : global::Android.Resource.Drawable.IcMediaPlay;

        var builder = new Notification.Builder(context, ChannelId)
            .SetContentTitle(media?.Title ?? "Tips Player")!
            .SetContentText(media != null ? $"{media.Artist} • {media.Album}" : "Ready to play")!
            .SetSmallIcon(global::Android.Resource.Drawable.IcMediaPlay)!
            .SetContentIntent(tapPending)!
            .SetOngoing(isPlaying)!
            .SetVisibility(NotificationVisibility.Public)!
            .SetCategory(Notification.CategoryTransport)!
            .AddAction(global::Android.Resource.Drawable.IcMediaPrevious, "Previous", prevIntent)!
            .AddAction(playPauseIcon, isPlaying ? "Pause" : "Play", ppIntent)!
            .AddAction(global::Android.Resource.Drawable.IcMediaNext, "Next", nextIntent)!
            .AddAction(global::Android.Resource.Drawable.IcDelete, "Stop", stopIntent);

        if (!string.IsNullOrEmpty(media?.AlbumArtPath) && File.Exists(media.AlbumArtPath))
        {
            try
            {
                var bmp = BitmapFactory.DecodeFile(media.AlbumArtPath);
                if (bmp != null) builder!.SetLargeIcon(bmp);
            }
            catch { /* non-fatal */ }
        }

        if (_mediaSession != null)
        {
            var style = new Notification.MediaStyle()
                .SetMediaSession(_mediaSession.SessionToken)!
                .SetShowActionsInCompactView(0, 1, 2);
            builder!.SetStyle(style);
        }

        return builder!.Build()!;
    }

    private void OnBridgeStateChanged()
    {
        UpdateMediaSessionMetadata();
        var notification = BuildNotification();
        var nm = (NotificationManager?)GetSystemService(NotificationService);
        nm?.Notify(NotificationId, notification);
    }

    // ── MediaSession callbacks (hardware buttons / Bluetooth) ─────────────────

    private sealed class MediaSessionCallback : MediaSession.Callback
    {
        public override void OnPlay()           => MediaServiceBridge.TriggerPlayPause();
        public override void OnPause()          => MediaServiceBridge.TriggerPlayPause();
        public override void OnSkipToNext()     => MediaServiceBridge.TriggerNext();
        public override void OnSkipToPrevious() => MediaServiceBridge.TriggerPrevious();
        public override void OnStop()           => MediaServiceBridge.TriggerStop();
    }
}
