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
        var logDirectory = Path.Combine(FileSystem.AppDataDirectory, "logs");
        Directory.CreateDirectory(logDirectory);

        var logFilePath = Path.Combine(logDirectory, "tipsplayer-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Debug(outputTemplate: DebugTemplate)
            .WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: LogFileTemplate,
                shared: true)
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog(Log.Logger, dispose: true);

        Log.Information("Application starting. Log directory: {LogDirectory}", logDirectory);

        return builder;
    }

    /// <summary>
    /// Gets the log file directory path.
    /// </summary>
    public static string LogDirectory => Path.Combine(FileSystem.AppDataDirectory, "logs");
}
