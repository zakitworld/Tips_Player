namespace Tips_Player.Constants;

/// <summary>
/// File name and path constants used throughout the application.
/// </summary>
public static class FileConstants
{
    /// <summary>
    /// Data file names.
    /// </summary>
    public static class DataFiles
    {
        /// <summary>
        /// Library data file name.
        /// </summary>
        public const string Library = "library.json";

        /// <summary>
        /// Listening statistics file name.
        /// </summary>
        public const string ListeningStats = "listening_stats.json";

        /// <summary>
        /// Play sessions file name.
        /// </summary>
        public const string PlaySessions = "play_sessions.json";

        /// <summary>
        /// Equalizer settings file name.
        /// </summary>
        public const string Equalizer = "equalizer.json";

        /// <summary>
        /// Application settings file name.
        /// </summary>
        public const string Settings = "settings.json";

        /// <summary>
        /// Playlists file name.
        /// </summary>
        public const string Playlists = "playlists.json";
    }

    /// <summary>
    /// Folder names.
    /// </summary>
    public static class Folders
    {
        /// <summary>
        /// Log files folder name.
        /// </summary>
        public const string Logs = "logs";

        /// <summary>
        /// Lyrics cache folder name.
        /// </summary>
        public const string LyricsCache = "lyrics_cache";

        /// <summary>
        /// Album art cache folder name.
        /// </summary>
        public const string AlbumArtCache = "album_art_cache";

        /// <summary>
        /// Temporary files folder name.
        /// </summary>
        public const string Temp = "temp";
    }

    /// <summary>
    /// Supported audio file extensions.
    /// </summary>
    public static readonly string[] AudioExtensions = [".mp3", ".wav", ".aac", ".m4a", ".flac", ".ogg", ".wma"];

    /// <summary>
    /// Supported video file extensions.
    /// </summary>
    public static readonly string[] VideoExtensions = [".mp4", ".avi", ".mkv", ".mov", ".wmv", ".webm", ".m4v"];

    /// <summary>
    /// Lyrics file extensions.
    /// </summary>
    public static readonly string[] LyricsExtensions = [".lrc", ".txt"];

    /// <summary>
    /// Log file pattern for rolling logs.
    /// </summary>
    public const string LogFilePattern = "tipsplayer-.log";
}
