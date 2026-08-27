namespace Tips_Player.Infrastructure.Persistence;

public interface IJsonFileStore
{
    Task<T?> ReadAsync<T>(string path, long maximumBytes, CancellationToken cancellationToken = default);
    Task WriteAsync<T>(string path, T value, long maximumBytes, CancellationToken cancellationToken = default);
}
