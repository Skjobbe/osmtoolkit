using Microsoft.Extensions.Logging;

namespace OsmToolkit.Models.Logging
{
    internal static partial class WayLogMessages
    {
        [LoggerMessage(Level = LogLevel.Trace, Message = "Way {WayId} with {NodeReferanceIdsCount} nodes with tags: [{Tags}]")]
        internal static partial void LogWaySummary(ILogger logger, long wayId, int nodeReferanceIdsCount, string tags);
        [LoggerMessage(Level = LogLevel.Trace, Message = "Way {WayId} with {NodeReferanceIdsCount} nodes")]
        internal static partial void LogWaySummaryTagless(ILogger logger, long wayId, int nodeReferanceIdsCount);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Way {WayId} created in v{Version}, CS {ChangeSet} by user {UserId} on {Timestamp}, visible={Visible} with tags: [{Tags}]")]
        internal static partial void LogWayCreationInfo(ILogger logger, long wayId, int version, long changeSet, long userId, DateTime timestamp, bool visible, string tags);

        [LoggerMessage(Level = LogLevel.Information, Message = "Way {WayId}: {NodeRefIdList} nodes, tags: [{Tags}]")]
        internal static partial void LogWayDetailed(ILogger logger, long wayId, string nodeRefIdList, string tags);


    }
}
