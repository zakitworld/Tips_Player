using Tips_Player.Models;

namespace Tips_Player.Services.Interfaces;

public interface IStatisticsService
{
    ListeningStats Stats { get; }

    Task LoadStatsAsync(CancellationToken cancellationToken = default);
    Task SaveStatsAsync(CancellationToken cancellationToken = default);
    Task RecordPlaySessionAsync(PlaySession session, CancellationToken cancellationToken = default);
    Task<ListeningStats> GetStatsForPeriodAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<List<TrackStats>> GetTopTracksAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<List<ArtistStats>> GetTopArtistsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<List<AlbumStats>> GetTopAlbumsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task ResetStatsAsync(CancellationToken cancellationToken = default);
}
