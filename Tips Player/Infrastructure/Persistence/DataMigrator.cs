using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Tips_Player.Infrastructure.Persistence;

/// <summary>
/// Handles data migration between schema versions.
/// </summary>
public class DataMigrator
{
    private readonly ILogger<DataMigrator> _logger;

    public DataMigrator(ILogger<DataMigrator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Migrates versioned data to the current version if needed.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    /// <param name="data">The versioned data to migrate.</param>
    /// <returns>The migrated data.</returns>
    public VersionedData<T> Migrate<T>(VersionedData<T> data) where T : class
    {
        if (!data.NeedsMigration)
        {
            _logger.LogDebug("Data is already at current version {Version}", data.Version);
            return data;
        }

        _logger.LogInformation("Migrating data from version {FromVersion} to {ToVersion}",
            data.Version, VersionedData<T>.CurrentVersion);

        var currentVersion = data.Version;

        while (currentVersion < VersionedData<T>.CurrentVersion)
        {
            _logger.LogDebug("Applying migration from version {Version}", currentVersion);
            data = ApplyMigration(data, currentVersion);
            currentVersion++;
        }

        data.Version = VersionedData<T>.CurrentVersion;
        data.Touch();

        _logger.LogInformation("Migration completed successfully to version {Version}", data.Version);
        return data;
    }

    /// <summary>
    /// Applies a single migration step.
    /// </summary>
    private VersionedData<T> ApplyMigration<T>(VersionedData<T> data, int fromVersion) where T : class
    {
        // Add migration logic here as needed
        // Example:
        // switch (fromVersion)
        // {
        //     case 1:
        //         return MigrateV1ToV2(data);
        //     case 2:
        //         return MigrateV2ToV3(data);
        //     default:
        //         throw new InvalidOperationException($"Unknown migration from version {fromVersion}");
        // }

        _logger.LogDebug("No specific migration needed for version {Version}", fromVersion);
        return data;
    }

    /// <summary>
    /// Reads versioned data from a file, migrating if necessary.
    /// </summary>
    public async Task<VersionedData<T>?> ReadAndMigrateAsync<T>(string filePath, CancellationToken cancellationToken = default) where T : class
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);

            // Try to read as versioned data first
            var versionedData = JsonSerializer.Deserialize<VersionedData<T>>(json);

            if (versionedData?.Data != null)
            {
                // It's versioned data, migrate if needed
                if (versionedData.NeedsMigration)
                {
                    versionedData = Migrate(versionedData);

                    // Save the migrated data
                    await WriteAsync(filePath, versionedData, cancellationToken);
                }

                return versionedData;
            }

            // Try to read as raw data (legacy format)
            var rawData = JsonSerializer.Deserialize<T>(json);
            if (rawData != null)
            {
                _logger.LogInformation("Converting legacy data format to versioned format");
                var wrapped = VersionedData<T>.Create(rawData);
                await WriteAsync(filePath, wrapped, cancellationToken);
                return wrapped;
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize data from {FilePath}", filePath);
        }

        return null;
    }

    /// <summary>
    /// Writes versioned data to a file.
    /// </summary>
    public async Task WriteAsync<T>(string filePath, VersionedData<T> data, CancellationToken cancellationToken = default) where T : class
    {
        data.Touch();
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    /// <summary>
    /// Creates a backup of a file before migration.
    /// </summary>
    public async Task<string?> CreateBackupAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var backupPath = $"{filePath}.backup.{DateTime.UtcNow:yyyyMMddHHmmss}";
        await Task.Run(() => File.Copy(filePath, backupPath), cancellationToken);

        _logger.LogInformation("Created backup at {BackupPath}", backupPath);
        return backupPath;
    }
}
