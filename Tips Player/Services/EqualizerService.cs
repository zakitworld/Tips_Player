using System.Text.Json;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.Services;

public class EqualizerService : IEqualizerService
{
    private const string SettingsFileName = "equalizer.json";
    private readonly string _settingsPath;

    private bool _isEnabled;
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                EnabledChanged?.Invoke(this, value);
                SaveSettings();
            }
        }
    }
    public EqualizerPreset CurrentPreset { get; set; } = EqualizerPreset.Flat;
    public AudioEffect AudioEffects { get; } = new();
    public List<EqualizerPreset> Presets { get; } = EqualizerPreset.GetAllPresets();
    public List<EqualizerPreset> CustomPresets { get; } = [];

    public event EventHandler<EqualizerPreset>? PresetChanged;
    public event EventHandler<bool>? EnabledChanged;

    public EqualizerService()
    {
        _settingsPath = Path.Combine(FileSystem.AppDataDirectory, SettingsFileName);
        LoadSettings();
    }

    public void ApplyPreset(EqualizerPreset preset)
    {
        CurrentPreset = preset.Clone();
        PresetChanged?.Invoke(this, CurrentPreset);
        SaveSettings();
    }

    public void SetBand(int bandIndex, double value)
    {
        var bands = CurrentPreset.GetBands();
        if (bandIndex >= 0 && bandIndex < bands.Length)
        {
            bands[bandIndex] = value;
            CurrentPreset.SetBands(bands);
            CurrentPreset.Name = "Custom";
            CurrentPreset.IsCustom = true;
            PresetChanged?.Invoke(this, CurrentPreset);
            SaveSettings();
        }
    }

    public void SaveCustomPreset(string name)
    {
        var preset = CurrentPreset.Clone();
        preset.Name = name;
        preset.IsCustom = true;
        preset.Icon = "\uf005"; // star

        var existing = CustomPresets.FirstOrDefault(p => p.Name == name);
        if (existing != null)
        {
            CustomPresets.Remove(existing);
        }

        CustomPresets.Add(preset);
        SaveSettings();
    }

    public void DeleteCustomPreset(EqualizerPreset preset)
    {
        CustomPresets.Remove(preset);
        SaveSettings();
    }

    public void LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath)) return;

            var json = File.ReadAllText(_settingsPath);
            var data = JsonSerializer.Deserialize<EqualizerData>(json);

            if (data != null)
            {
                _isEnabled = data.IsEnabled;
                CurrentPreset.SetBands(data.CurrentBands);
                CurrentPreset.Name = data.CurrentPresetName;

                AudioEffects.BassBoostEnabled = data.BassBoostEnabled;
                AudioEffects.BassBoostStrength = data.BassBoostStrength;
                AudioEffects.VirtualizerEnabled = data.VirtualizerEnabled;
                AudioEffects.VirtualizerStrength = data.VirtualizerStrength;
                AudioEffects.ReverbEnabled = data.ReverbEnabled;
                AudioEffects.ReverbPreset = data.ReverbPreset;

                CustomPresets.Clear();
                foreach (var presetData in data.CustomPresets)
                {
                    var preset = new EqualizerPreset
                    {
                        Name = presetData.Name,
                        Icon = "\uf005",
                        IsCustom = true
                    };
                    preset.SetBands(presetData.Bands);
                    CustomPresets.Add(preset);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading equalizer settings: {ex.Message}");
        }
    }

    public void SaveSettings()
    {
        try
        {
            var data = new EqualizerData
            {
                IsEnabled = IsEnabled,
                CurrentPresetName = CurrentPreset.Name,
                CurrentBands = CurrentPreset.GetBands(),
                BassBoostEnabled = AudioEffects.BassBoostEnabled,
                BassBoostStrength = AudioEffects.BassBoostStrength,
                VirtualizerEnabled = AudioEffects.VirtualizerEnabled,
                VirtualizerStrength = AudioEffects.VirtualizerStrength,
                ReverbEnabled = AudioEffects.ReverbEnabled,
                ReverbPreset = AudioEffects.ReverbPreset,
                CustomPresets = CustomPresets.Select(p => new PresetData
                {
                    Name = p.Name,
                    Bands = p.GetBands()
                }).ToList()
            };

            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving equalizer settings: {ex.Message}");
        }
    }

    private class EqualizerData
    {
        public bool IsEnabled { get; set; }
        public string CurrentPresetName { get; set; } = "Flat";
        public double[] CurrentBands { get; set; } = new double[10];
        public bool BassBoostEnabled { get; set; }
        public double BassBoostStrength { get; set; }
        public bool VirtualizerEnabled { get; set; }
        public double VirtualizerStrength { get; set; }
        public bool ReverbEnabled { get; set; }
        public string ReverbPreset { get; set; } = "None";
        public List<PresetData> CustomPresets { get; set; } = [];
    }

    private class PresetData
    {
        public string Name { get; set; } = string.Empty;
        public double[] Bands { get; set; } = new double[10];
    }
}
