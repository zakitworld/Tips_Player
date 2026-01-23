using Tips_Player.Models;

namespace Tips_Player.Services.Interfaces;

public interface IEqualizerService
{
    bool IsEnabled { get; set; }
    EqualizerPreset CurrentPreset { get; set; }
    AudioEffect AudioEffects { get; }
    List<EqualizerPreset> Presets { get; }
    List<EqualizerPreset> CustomPresets { get; }

    event EventHandler<EqualizerPreset>? PresetChanged;
    event EventHandler<bool>? EnabledChanged;

    void ApplyPreset(EqualizerPreset preset);
    void SetBand(int bandIndex, double value);
    void SaveCustomPreset(string name);
    void DeleteCustomPreset(EqualizerPreset preset);
    void LoadSettings();
    void SaveSettings();
}
