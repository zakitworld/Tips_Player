using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.ViewModels;

public partial class EqualizerViewModel : BaseViewModel
{
    private readonly IEqualizerService _equalizerService;

    [ObservableProperty]
    private bool _isEnabled;

    [ObservableProperty]
    private EqualizerPreset _currentPreset;

    [ObservableProperty]
    private ObservableCollection<EqualizerPreset> _presets = [];

    [ObservableProperty]
    private ObservableCollection<EqualizerPreset> _customPresets = [];

    [ObservableProperty]
    private string _newPresetName = string.Empty;

    [ObservableProperty]
    private bool _isCustomPresetDialogVisible;

    // Individual band properties for binding
    [ObservableProperty]
    private double _band32Hz;

    [ObservableProperty]
    private double _band64Hz;

    [ObservableProperty]
    private double _band125Hz;

    [ObservableProperty]
    private double _band250Hz;

    [ObservableProperty]
    private double _band500Hz;

    [ObservableProperty]
    private double _band1kHz;

    [ObservableProperty]
    private double _band2kHz;

    [ObservableProperty]
    private double _band4kHz;

    [ObservableProperty]
    private double _band8kHz;

    [ObservableProperty]
    private double _band16kHz;

    // Audio Effects
    [ObservableProperty]
    private bool _bassBoostEnabled;

    [ObservableProperty]
    private double _bassBoostStrength;

    [ObservableProperty]
    private bool _virtualizerEnabled;

    [ObservableProperty]
    private double _virtualizerStrength;

    public string[] BandLabels { get; } = ["32", "64", "125", "250", "500", "1K", "2K", "4K", "8K", "16K"];

    public EqualizerViewModel(IEqualizerService equalizerService)
    {
        _equalizerService = equalizerService;
        _currentPreset = _equalizerService.CurrentPreset;
        Title = "Equalizer";

        LoadData();
    }

    private void LoadData()
    {
        IsEnabled = _equalizerService.IsEnabled;
        CurrentPreset = _equalizerService.CurrentPreset;
        UpdateBandsFromPreset();

        Presets = new ObservableCollection<EqualizerPreset>(_equalizerService.Presets);
        CustomPresets = new ObservableCollection<EqualizerPreset>(_equalizerService.CustomPresets);

        BassBoostEnabled = _equalizerService.AudioEffects.BassBoostEnabled;
        BassBoostStrength = _equalizerService.AudioEffects.BassBoostStrength;
        VirtualizerEnabled = _equalizerService.AudioEffects.VirtualizerEnabled;
        VirtualizerStrength = _equalizerService.AudioEffects.VirtualizerStrength;
    }

    private void UpdateBandsFromPreset()
    {
        Band32Hz = CurrentPreset.Band32Hz;
        Band64Hz = CurrentPreset.Band64Hz;
        Band125Hz = CurrentPreset.Band125Hz;
        Band250Hz = CurrentPreset.Band250Hz;
        Band500Hz = CurrentPreset.Band500Hz;
        Band1kHz = CurrentPreset.Band1kHz;
        Band2kHz = CurrentPreset.Band2kHz;
        Band4kHz = CurrentPreset.Band4kHz;
        Band8kHz = CurrentPreset.Band8kHz;
        Band16kHz = CurrentPreset.Band16kHz;
    }

    partial void OnIsEnabledChanged(bool value)
    {
        _equalizerService.IsEnabled = value;
        _equalizerService.SaveSettings();
    }

    partial void OnBand32HzChanged(double value) => UpdateBand(0, value);
    partial void OnBand64HzChanged(double value) => UpdateBand(1, value);
    partial void OnBand125HzChanged(double value) => UpdateBand(2, value);
    partial void OnBand250HzChanged(double value) => UpdateBand(3, value);
    partial void OnBand500HzChanged(double value) => UpdateBand(4, value);
    partial void OnBand1kHzChanged(double value) => UpdateBand(5, value);
    partial void OnBand2kHzChanged(double value) => UpdateBand(6, value);
    partial void OnBand4kHzChanged(double value) => UpdateBand(7, value);
    partial void OnBand8kHzChanged(double value) => UpdateBand(8, value);
    partial void OnBand16kHzChanged(double value) => UpdateBand(9, value);

    private void UpdateBand(int index, double value)
    {
        _equalizerService.SetBand(index, value);
        CurrentPreset = _equalizerService.CurrentPreset;
    }

    partial void OnBassBoostEnabledChanged(bool value)
    {
        _equalizerService.AudioEffects.BassBoostEnabled = value;
        _equalizerService.SaveSettings();
    }

    partial void OnBassBoostStrengthChanged(double value)
    {
        _equalizerService.AudioEffects.BassBoostStrength = value;
        _equalizerService.SaveSettings();
    }

    partial void OnVirtualizerEnabledChanged(bool value)
    {
        _equalizerService.AudioEffects.VirtualizerEnabled = value;
        _equalizerService.SaveSettings();
    }

    partial void OnVirtualizerStrengthChanged(double value)
    {
        _equalizerService.AudioEffects.VirtualizerStrength = value;
        _equalizerService.SaveSettings();
    }

    [RelayCommand]
    private void SelectPreset(EqualizerPreset? preset)
    {
        if (preset == null) return;

        _equalizerService.ApplyPreset(preset);
        CurrentPreset = _equalizerService.CurrentPreset;
        UpdateBandsFromPreset();
    }

    [RelayCommand]
    private void ResetToFlat()
    {
        SelectPreset(EqualizerPreset.Flat);
    }

    [RelayCommand]
    private void ShowSavePresetDialog()
    {
        NewPresetName = string.Empty;
        IsCustomPresetDialogVisible = true;
    }

    [RelayCommand]
    private void HideSavePresetDialog()
    {
        IsCustomPresetDialogVisible = false;
    }

    [RelayCommand]
    private void SaveCustomPreset()
    {
        if (string.IsNullOrWhiteSpace(NewPresetName)) return;

        _equalizerService.SaveCustomPreset(NewPresetName);
        CustomPresets = new ObservableCollection<EqualizerPreset>(_equalizerService.CustomPresets);
        IsCustomPresetDialogVisible = false;
        NewPresetName = string.Empty;
    }

    [RelayCommand]
    private void DeleteCustomPreset(EqualizerPreset? preset)
    {
        if (preset == null) return;

        _equalizerService.DeleteCustomPreset(preset);
        CustomPresets = new ObservableCollection<EqualizerPreset>(_equalizerService.CustomPresets);
    }
}
