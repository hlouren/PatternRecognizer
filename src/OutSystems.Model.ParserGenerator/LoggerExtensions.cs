using Microsoft.Extensions.Logging;

namespace OutSystems.Model.ParserGenerator;

internal static class LoggerExtensions {

    public static void WriteLine(this ILogger? logger, string message = "") => logger?.LogTrace(message);
}
