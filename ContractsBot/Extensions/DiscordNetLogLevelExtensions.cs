using Discord;

namespace ContractsBot.Extensions;

internal static class DiscordNetLogLevelExtensions
{
    public static LogLevel ToLogLevel(this LogSeverity severity) => severity switch
    {
        LogSeverity.Critical => LogLevel.Critical,
        LogSeverity.Error => LogLevel.Error,
        LogSeverity.Warning => LogLevel.Warning,
        LogSeverity.Info => LogLevel.Information,
        LogSeverity.Verbose => LogLevel.Trace,
        LogSeverity.Debug => LogLevel.Debug,
        _ => LogLevel.Critical
    };
}
