using Android.Content;
using Android.Media;

namespace Tips_Player.Platforms.Android.Services;

/// <summary>
/// Requests, maintains, and releases Android audio focus.
/// Pauses playback on focus loss, ducks volume for transient interruptions.
/// </summary>
public sealed class AudioFocusManager : Java.Lang.Object, AudioManager.IOnAudioFocusChangeListener
{
    private readonly AudioManager _audioManager;
    private AudioFocusRequest? _focusRequest;
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
                .Build();
            result = _audioManager.RequestAudioFocus(_focusRequest);
        }
        else
        {
#pragma warning disable CA1422
            result = _audioManager.RequestAudioFocus(this, Stream.Music, AudioFocus.Gain);
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
                Duck?.Invoke(0.25f); // lower to 25 % while another app is temporarily loud
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
