using Android.Media.Audiofx;
using Microsoft.Extensions.Logging;
using Tips_Player.Models;
using Tips_Player.Services;
using Tips_Player.Services.Interfaces;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

namespace Tips_Player.Platforms.Android.Services;

/// <summary>
/// Wraps <see cref="EqualizerService"/> (settings/preset persistence) and
/// additionally drives Android's hardware <see cref="Equalizer"/>, <see cref="BassBoost"/>,
/// and <see cref="Virtualizer"/> AudioEffect objects in real-time.
/// </summary>
public class AndroidEqualizerService : EqualizerService, IDisposable
{
    private Equalizer?   _eq;
    private BassBoost?   _bass;
    private Virtualizer? _virt;
    private bool         _hwInitialized;
    private readonly ILogger<AndroidEqualizerService> _logger;

    // Android EQ range in milliBels (typically -1500 to +1500)
    private short _minMb;
    private short _maxMb;

    public AndroidEqualizerService(ILogger<AndroidEqualizerService> logger,
                                   ILoggerFactory loggerFactory)
        : base(loggerFactory.CreateLogger<EqualizerService>())
    {
        _logger = logger;
    }

    // ── Initialization ────────────────────────────────────────────────────────

    /// <summary>
    /// Call once the ExoPlayer audio session is known (0 = global, applies to all audio).
    /// Safe to call multiple times — re-initialises if the session changes.
    /// </summary>
    public void InitializeHardware(int audioSessionId = 0)
    {
        DisposeHardware();
        try
        {
            _eq   = new Equalizer(0, audioSessionId);
            _bass = new BassBoost(0, audioSessionId);
            _virt = new Virtualizer(0, audioSessionId);

            _eq.SetEnabled(IsEnabled);
            _bass.SetEnabled(IsEnabled && AudioEffects.BassBoostEnabled);
            _virt.SetEnabled(IsEnabled && AudioEffects.VirtualizerEnabled);

            var range = _eq.BandLevelRange!;
            _minMb = range[0];
            _maxMb = range[1];

            _hwInitialized = true;
            _logger.LogInformation("Android hardware EQ initialised (session={S}, bands={B}, range={Min}..{Max} mB)",
                audioSessionId, _eq.NumberOfBands, _minMb, _maxMb);

            // Push current preset into hardware immediately
            SyncAllBandsToHardware();
            SyncEffectsToHardware();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Hardware EQ init failed — software-only mode");
            _hwInitialized = false;
        }
    }

    // ── IEqualizerService overrides ───────────────────────────────────────────

    public override void ApplyPreset(EqualizerPreset preset)
    {
        base.ApplyPreset(preset);
        if (_hwInitialized) SyncAllBandsToHardware();
    }

    public override void SetBand(int bandIndex, double value)
    {
        base.SetBand(bandIndex, value);
        if (_hwInitialized) SetHardwareBand(bandIndex, value);
    }

    // Intercept AudioEffects changes
    public new bool IsEnabled
    {
        get => base.IsEnabled;
        set
        {
            base.IsEnabled = value;
            if (_hwInitialized)
            {
                _eq?.SetEnabled(value);
                _bass?.SetEnabled(value && AudioEffects.BassBoostEnabled);
                _virt?.SetEnabled(value && AudioEffects.VirtualizerEnabled);
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SyncAllBandsToHardware()
    {
        var bands = CurrentPreset.GetBands();
        for (int i = 0; i < bands.Length && i < (_eq?.NumberOfBands ?? 0); i++)
            SetHardwareBand(i, bands[i]);
    }

    private void SetHardwareBand(int bandIndex, double value)
    {
        if (_eq == null) return;
        // UI range is -12..+12 dB; convert to milliBels for the hardware
        var mb = (short)Math.Clamp(
            value / 12.0 * (_maxMb - _minMb) / 2 + (_minMb + _maxMb) / 2.0,
            _minMb, _maxMb);
        try { _eq.SetBandLevel((short)bandIndex, mb); }
        catch (Exception ex) { _logger.LogWarning(ex, "SetBandLevel failed for band {B}", bandIndex); }
    }

    private void SyncEffectsToHardware()
    {
        if (_bass != null)
        {
            _bass.SetEnabled(IsEnabled && AudioEffects.BassBoostEnabled);
            // Strength 0-1000
            var strength = (short)Math.Clamp(AudioEffects.BassBoostStrength * 10, 0, 1000);
            try { _bass.SetStrength(strength); } catch { /* non-fatal */ }
        }

        if (_virt != null)
        {
            _virt.SetEnabled(IsEnabled && AudioEffects.VirtualizerEnabled);
            var strength = (short)Math.Clamp(AudioEffects.VirtualizerStrength * 10, 0, 1000);
            try { _virt.SetStrength(strength); } catch { /* non-fatal */ }
        }
    }

    // ── Disposal ──────────────────────────────────────────────────────────────

    private void DisposeHardware()
    {
        try { _eq?.Release();   _eq?.Dispose();   } catch { /* non-fatal */ }
        try { _bass?.Release(); _bass?.Dispose(); } catch { /* non-fatal */ }
        try { _virt?.Release(); _virt?.Dispose(); } catch { /* non-fatal */ }
        _eq   = null;
        _bass = null;
        _virt = null;
        _hwInitialized = false;
    }

    public new void Dispose()
    {
        DisposeHardware();
        base.Dispose();
    }
}
