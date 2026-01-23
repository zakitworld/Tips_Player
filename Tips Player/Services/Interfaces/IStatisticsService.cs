using Tips_Player.Models;

namespace Tips_Player.Services.Interfaces;

public interface IStatisticsService
{
    ListeningStats Stats { get; }

    Task LoadStatsAsync();
    Task SaveStatsAsync();
    Task RecordPlaySessionAsync(PlaySession session);
    Task<ListeningStats> GetStatsForPeriodAsync(DateTime start, DateTime end);
    Task<List<TrackStats>> GetTopTracksAsync(int count = 10);
    Task<List<ArtistStats>> GetTopArtistsAsync(int count = 10);
    Task<List<AlbumStats>> GetTopAlbumsAsync(int count = 10);
    Task ResetStatsAsync();
}
