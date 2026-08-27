using System.Text;
using System.Text.Json;

namespace Tips_Player.Infrastructure.Persistence;

public sealed class AtomicJsonFileStore : IJsonFileStore
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _options = new()
    {
        MaxDepth = 32,
        PropertyNameCaseInsensitive = false
    };

    public async Task<T?> ReadAsync<T>(string path, long maximumBytes, CancellationToken cancellationToken = default)
    {
        ValidateArguments(path, maximumBytes);
        if (!File.Exists(path)) return default;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 16 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
            if (stream.Length > maximumBytes)
                throw new InvalidDataException($"JSON data exceeds the {maximumBytes}-byte limit.");

            return await JsonSerializer.DeserializeAsync<T>(stream, _options, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task WriteAsync<T>(string path, T value, long maximumBytes, CancellationToken cancellationToken = default)
    {
        ValidateArguments(path, maximumBytes);
        ArgumentNullException.ThrowIfNull(value);

        var json = JsonSerializer.Serialize(value, _options);
        if (Encoding.UTF8.GetByteCount(json) > maximumBytes)
            throw new InvalidDataException($"JSON data exceeds the {maximumBytes}-byte limit.");

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        var temporaryPath = path + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temporaryPath, json, cancellationToken).ConfigureAwait(false);
            File.Move(temporaryPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            _gate.Release();
        }
    }

    private static void ValidateArguments(string path, long maximumBytes)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("A data path is required.", nameof(path));
        if (!Path.IsPathFullyQualified(path)) throw new ArgumentException("Data path must be absolute.", nameof(path));
        if (maximumBytes <= 0) throw new ArgumentOutOfRangeException(nameof(maximumBytes));
    }
}
