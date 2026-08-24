using Microsoft.Extensions.Logging;

namespace OsmToolkit.Mcp.Tools.Logging
{
    internal static partial class SearchByTagsInAreaLogMessages
    {
        [LoggerMessage(LogLevel.Debug, Message = "Searching for {TagCount} tag filter(s) in \"{Place}\".")]
        internal static partial void LogSearchStart(ILogger logger, string place, int tagCount);

        [LoggerMessage(LogLevel.Debug, Message = "Found {MatchCount} match(es) for \"{Place}\".")]
        internal static partial void LogSearchResult(ILogger logger, string place, int matchCount);
    }
}
