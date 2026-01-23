using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.ViewModels;

public partial class StatisticsViewModel : BaseViewModel
{
    private readonly IStatisticsService _statisticsService;

    [ObservableProperty]
    private ListeningStats _stats = new();

    [ObservableProperty]
    private ObservableCollection<TrackStats> _topTracks = [];

    [ObservableProperty]
    private ObservableCollection<ArtistStats> _topArtists = [];

    [ObservableProperty]
    private ObservableCollection<AlbumStats> _topAlbums = [];

    [ObservableProperty]
    private string _selectedPeriod = "All Time";

    [ObservableProperty]
    private int _selectedTabIndex;

    public ObservableCollection<string> Periods { get; } = ["Today", "This Week", "This Month", "This Year", "All Time"];

    public StatisticsViewModel(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
        Title = "Statistics";
    }

    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            await _statisticsService.LoadStatsAsync();
            await RefreshStatsAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSelectedPeriodChanged(string value)
    {
        _ = RefreshStatsAsync();
    }

    private async Task RefreshStatsAsync()
    {
        var (start, end) = GetPeriodRange(SelectedPeriod);

        if (SelectedPeriod == "All Time")
        {
            Stats = _statisticsService.Stats;
        }
        else
        {
            Stats = await _statisticsService.GetStatsForPeriodAsync(start, end);
        }

        TopTracks = new ObservableCollection<TrackStats>(Stats.TopTracks.Take(10));
        TopArtists = new ObservableCollection<ArtistStats>(Stats.TopArtists.Take(10));
        TopAlbums = new ObservableCollection<AlbumStats>(Stats.TopAlbums.Take(10));
    }

    private static (DateTime start, DateTime end) GetPeriodRange(string period)
    {
        var now = DateTime.Now;
        return period switch
        {
            "Today" => (now.Date, now),
            "This Week" => (now.AddDays(-(int)now.DayOfWeek), now),
            "This Month" => (new DateTime(now.Year, now.Month, 1), now),
            "This Year" => (new DateTime(now.Year, 1, 1), now),
            _ => (DateTime.MinValue, now)
        };
    }

    [RelayCommand]
    private async Task ResetStatsAsync()
    {
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Reset Statistics",
            "Are you sure you want to reset all listening statistics? This cannot be undone.",
            "Reset",
            "Cancel");

        if (confirm)
        {
            await _statisticsService.ResetStatsAsync();
            await RefreshStatsAsync();
        }
    }
}
