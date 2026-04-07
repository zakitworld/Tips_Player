using Android.Content;
using Android.Media;

// All code in this file targets Android only. CA1416 calls are already guarded by
// Build.VERSION.SdkInt >= BuildVersionCodes.O at runtime.
#pragma warning disable CA1416, CS8602, CS8604  // Android builder chains are non-null by contract

namespace Tips_Player.Platforms.Android.Services;

/// <summary>
/// Requests, maintains, and releases Android audio focus.
/// Pauses playback on focus loss, ducks volume for transient interruptions.
/// </summary>
public sealed class AudioFocusManager : Java.Lang.Object, AudioManager.IOnAudioFocusChangeListener
{
    private readonly AudioManager _audioManager;
    private AudioFocusRequestClass? _focusRequest;
    private bool _hasFocus;

    /// <summary>Fired when audio focus is fully regained (resume + restore volume).</summary>
    public event Action? FocusGained;

    /// <summary>Fired when focus is lost — pause playback.</summary>
    public event Action? FocusLost;

    /// <summary>Fired with a 0-1 volume multiplier for ducking (transient).</summary>
    public event Action<float>? Duck;

    public AudioFocusManager()
    {
        _audioManager = (AudioManager)global::Android.App.Application.Context
            .GetSystemService(Context.AudioService)!;
    }

    public bool RequestFocus()
    {
        if (_hasFocus) return true;

        AudioFocusRequest result;

        if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.O)
        {
            _focusRequest = new AudioFocusRequestClass.Builder(AudioFocus.Gain)
                .SetAudioAttributes(new AudioAttributes.Builder()
                    .SetUsage(AudioUsageKind.Media)
                    .SetContentType(AudioContentType.Music)
                    .Build()!)
                .SetOnAudioFocusChangeListener(this)
                .SetWillPauseWhenDucked(false)
                .Build()!;   // Build() is non-null when builder is fully configured
            result = _audioManager.RequestAudioFocus(_focusRequest!);
        }
        else
        {
#pragma warning disable CA1422
            result = _audioManager.RequestAudioFocus(this, global::Android.Media.Stream.Music, AudioFocus.Gain);
#pragma warning restore CA1422
        }

        _hasFocus = result == AudioFocusRequest.Granted;
        return _hasFocus;
    }

    public void AbandonFocus()
    {
        if (!_hasFocus) return;

        if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.O &&
            _focusRequest != null)
        {
            _audioManager.AbandonAudioFocusRequest(_focusRequest);
        }
        else
        {
#pragma warning disable CA1422
            _audioManager.AbandonAudioFocus(this);
#pragma warning restore CA1422
        }

        _hasFocus = false;
    }

    public void OnAudioFocusChange(AudioFocus focusChange)
    {
        switch (focusChange)
        {
            case AudioFocus.Gain:
                _hasFocus = true;
                Duck?.Invoke(1.0f);
                FocusGained?.Invoke();
                break;

            case AudioFocus.LossTransientCanDuck:
                Duck?.Invoke(0.25f);
                break;

            case AudioFocus.LossTransient:
                FocusLost?.Invoke();
                break;

            case AudioFocus.Loss:
                _hasFocus = false;
                FocusLost?.Invoke();
                AbandonFocus();
                break;
        }
    }
}
