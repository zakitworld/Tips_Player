using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Tips_Player.Infrastructure.Logging;

/// <summary>
/// Configures Serilog logging for the application.
/// </summary>
public static class LoggingConfiguration
{
    private const string LogFileTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
    private const string DebugTemplate = "[{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

    /// <summary>
    /// Configures Serilog with file and debug sinks.
    /// </summary>
    /// <param name="builder">The MAUI app builder.</param>
    /// <returns>The configured builder.</returns>
    public static MauiAppBuilder ConfigureSerilog(this MauiAppBuilder builder)
    {
        try
        {
            var appDataDir = FileSystem.AppDataDirectory;
            if (string.IsNullOrEmpty(appDataDir))
            {
                throw new InvalidOperationException("AppDataDirectory is not available during early startup.");
            }

            var logDirectory = Path.Combine(appDataDir, "logs");
            Directory.CreateDirectory(logDirectory);

            var logFilePath = Path.Combine(logDirectory, "tipsplayer-.log");

            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Debug(outputTemplate: DebugTemplate)
                .WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: LogFileTemplate);

            Log.Logger = logConfig.CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger, dispose: true);

            Log.Information("Application starting. Log directory: {LogDirectory}", logDirectory);
        }
        catch (Exception ex)
        {
            // Fall back to debug-only logging if file sink fails (e.g. on Android with restricted paths)
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug(outputTemplate: DebugTemplate)
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger, dispose: true);

            Log.Warning(ex, "File logging unavailable; using debug sink only");
        }

        return builder;
    }

    /// <summary>
    /// Gets the log file directory path.
    /// </summary>
    public static string LogDirectory => Path.Combine(FileSystem.AppDataDirectory, "logs");
}
