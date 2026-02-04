using System.Text.Json.Serialization;

namespace Tips_Player.Infrastructure.Persistence;

/// <summary>
/// Wrapper for data that includes version information for migration support.
/// </summary>
/// <typeparam name="T">The type of data being wrapped.</typeparam>
public class VersionedData<T> where T : class
{
    /// <summary>
    /// Current data schema version. Increment when making breaking changes.
    /// </summary>
    public const int CurrentVersion = 1;

    /// <summary>
    /// The schema version of this data.
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; } = CurrentVersion;

    /// <summary>
    /// When the data was last modified.
    /// </summary>
    [JsonPropertyName("lastModified")]
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The actual data payload.
    /// </summary>
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    /// <summary>
    /// Optional metadata about the data.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Creates a new versioned data wrapper with the current version.
    /// </summary>
    public static VersionedData<T> Create(T data, Dictionary<string, string>? metadata = null)
    {
        return new VersionedData<T>
        {
            Version = CurrentVersion,
            LastModified = DateTime.UtcNow,
            Data = data,
            Metadata = metadata
        };
    }

    /// <summary>
    /// Checks if this data needs migration.
    /// </summary>
    public bool NeedsMigration => Version < CurrentVersion;

    /// <summary>
    /// Updates the modification timestamp.
    /// </summary>
    public void Touch()
    {
        LastModified = DateTime.UtcNow;
    }
}

/// <summary>
/// Data version history for tracking schema changes.
/// </summary>
public static class DataVersionHistory
{
    /// <summary>
    /// Version 1: Initial schema.
    /// </summary>
    public const int V1_Initial = 1;

    /// <summary>
    /// Gets a description of what changed in each version.
    /// </summary>
    public static string GetVersionDescription(int version) => version switch
    {
        V1_Initial => "Initial data schema",
        _ => "Unknown version"
    };
}
