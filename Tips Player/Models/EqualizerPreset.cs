using CommunityToolkit.Mvvm.ComponentModel;

namespace Tips_Player.Models;

public partial class EqualizerPreset : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private bool _isCustom;

    // 10-band equalizer: 32Hz, 64Hz, 125Hz, 250Hz, 500Hz, 1kHz, 2kHz, 4kHz, 8kHz, 16kHz
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

    public double[] GetBands() => [Band32Hz, Band64Hz, Band125Hz, Band250Hz, Band500Hz, Band1kHz, Band2kHz, Band4kHz, Band8kHz, Band16kHz];

    public void SetBands(double[] bands)
    {
        if (bands.Length >= 10)
        {
            Band32Hz = bands[0];
            Band64Hz = bands[1];
            Band125Hz = bands[2];
            Band250Hz = bands[3];
            Band500Hz = bands[4];
            Band1kHz = bands[5];
            Band2kHz = bands[6];
            Band4kHz = bands[7];
            Band8kHz = bands[8];
            Band16kHz = bands[9];
        }
    }

    public static EqualizerPreset Flat => new()
    {
        Name = "Flat",
        Icon = "\uf068",
        Band32Hz = 0, Band64Hz = 0, Band125Hz = 0, Band250Hz = 0, Band500Hz = 0,
        Band1kHz = 0, Band2kHz = 0, Band4kHz = 0, Band8kHz = 0, Band16kHz = 0
    };

    public static EqualizerPreset BassBoost => new()
    {
        Name = "Bass Boost",
        Icon = "\uf0e7",
        Band32Hz = 6, Band64Hz = 5, Band125Hz = 4, Band250Hz = 2, Band500Hz = 1,
        Band1kHz = 0, Band2kHz = 0, Band4kHz = 0, Band8kHz = 0, Band16kHz = 0
    };

    public static EqualizerPreset TrebleBoost => new()
    {
        Name = "Treble Boost",
        Icon = "\uf001",
        Band32Hz = 0, Band64Hz = 0, Band125Hz = 0, Band250Hz = 0, Band500Hz = 0,
        Band1kHz = 1, Band2kHz = 2, Band4kHz = 4, Band8kHz = 5, Band16kHz = 6
    };

    public static EqualizerPreset Rock => new()
    {
        Name = "Rock",
        Icon = "\uf3b5",
        Band32Hz = 5, Band64Hz = 4, Band125Hz = 2, Band250Hz = -1, Band500Hz = -2,
        Band1kHz = 1, Band2kHz = 3, Band4kHz = 4, Band8kHz = 5, Band16kHz = 5
    };

    public static EqualizerPreset Pop => new()
    {
        Name = "Pop",
        Icon = "\uf130",
        Band32Hz = -1, Band64Hz = 1, Band125Hz = 3, Band250Hz = 4, Band500Hz = 3,
        Band1kHz = 1, Band2kHz = -1, Band4kHz = -2, Band8kHz = -1, Band16kHz = -1
    };

    public static EqualizerPreset Jazz => new()
    {
        Name = "Jazz",
        Icon = "\uf001",
        Band32Hz = 3, Band64Hz = 2, Band125Hz = 1, Band250Hz = 2, Band500Hz = -2,
        Band1kHz = -2, Band2kHz = 0, Band4kHz = 1, Band8kHz = 3, Band16kHz = 4
    };

    public static EqualizerPreset Classical => new()
    {
        Name = "Classical",
        Icon = "\uf001",
        Band32Hz = 4, Band64Hz = 3, Band125Hz = 2, Band250Hz = 1, Band500Hz = -1,
        Band1kHz = -1, Band2kHz = 0, Band4kHz = 2, Band8kHz = 3, Band16kHz = 4
    };

    public static EqualizerPreset HipHop => new()
    {
        Name = "Hip Hop",
        Icon = "\uf001",
        Band32Hz = 5, Band64Hz = 5, Band125Hz = 3, Band250Hz = 1, Band500Hz = -1,
        Band1kHz = -1, Band2kHz = 1, Band4kHz = 0, Band8kHz = 2, Band16kHz = 3
    };

    public static EqualizerPreset Electronic => new()
    {
        Name = "Electronic",
        Icon = "\uf001",
        Band32Hz = 5, Band64Hz = 4, Band125Hz = 2, Band250Hz = 0, Band500Hz = -2,
        Band1kHz = 1, Band2kHz = 0, Band4kHz = 2, Band8kHz = 4, Band16kHz = 5
    };

    public static EqualizerPreset Vocal => new()
    {
        Name = "Vocal",
        Icon = "\uf130",
        Band32Hz = -2, Band64Hz = -1, Band125Hz = 0, Band250Hz = 2, Band500Hz = 4,
        Band1kHz = 4, Band2kHz = 3, Band4kHz = 2, Band8kHz = 0, Band16kHz = -1
    };

    public static EqualizerPreset Loudness => new()
    {
        Name = "Loudness",
        Icon = "\uf028",
        Band32Hz = 5, Band64Hz = 4, Band125Hz = 2, Band250Hz = 0, Band500Hz = -1,
        Band1kHz = -1, Band2kHz = 0, Band4kHz = 2, Band8kHz = 4, Band16kHz = 5
    };

    public static List<EqualizerPreset> GetAllPresets() =>
    [
        Flat, BassBoost, TrebleBoost, Rock, Pop, Jazz, Classical, HipHop, Electronic, Vocal, Loudness
    ];

    public EqualizerPreset Clone() => new()
    {
        Name = Name,
        Icon = Icon,
        IsCustom = IsCustom,
        Band32Hz = Band32Hz,
        Band64Hz = Band64Hz,
        Band125Hz = Band125Hz,
        Band250Hz = Band250Hz,
        Band500Hz = Band500Hz,
        Band1kHz = Band1kHz,
        Band2kHz = Band2kHz,
        Band4kHz = Band4kHz,
        Band8kHz = Band8kHz,
        Band16kHz = Band16kHz
    };
}

public partial class AudioEffect : ObservableObject
{
    [ObservableProperty]
    private bool _bassBoostEnabled;

    [ObservableProperty]
    private double _bassBoostStrength = 50;

    [ObservableProperty]
    private bool _virtualizerEnabled;

    [ObservableProperty]
    private double _virtualizerStrength = 50;

    [ObservableProperty]
    private bool _reverbEnabled;

    [ObservableProperty]
    private string _reverbPreset = "None";
}
