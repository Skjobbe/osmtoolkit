using Microsoft.Extensions.Logging;

namespace OsmToolkit.Mcp.Tools.Logging
{
    internal static partial class FindNearPointLogMessages
    {
        [LoggerMessage(LogLevel.Debug, Message = "Finding up to {Limit} node(s) within {RadiusMeters}m of \"{Place}\".")]
        internal static partial void LogSearchStart(ILogger logger, string place, double radiusMeters, int limit);

        [LoggerMessage(LogLevel.Debug, Message = "Found {MatchCount} node(s) near \"{Place}\".")]
        internal static partial void LogSearchResult(ILogger logger, string place, int matchCount);
    }
}
