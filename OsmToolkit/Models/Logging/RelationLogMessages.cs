using Microsoft.Extensions.Logging;

namespace OsmToolkit.Models.Logging
{
    internal static partial class RelationLogMessages
    {
        [LoggerMessage(Level = LogLevel.Trace, Message = "Relation {RelationId} with {MemberCount} members with tags: [{Tags}]")]
        internal static partial void LogRelationSummary(ILogger logger, long relationId, int memberCount, string tags);
        [LoggerMessage(Level = LogLevel.Trace, Message = "Relation {RelationId} with {MemberCount} members")]
        internal static partial void LogRelationSummaryTagless(ILogger logger, long relationId, int memberCount);

        [LoggerMessage(Level = LogLevel.Debug, Message = "Relation {RelationId} with {MemberCount} member, created in v{Version}, CS {ChangeSet} by {User} on {Timestamp}, visible={Visible} with tags: [{Tags}]")]
        internal static partial void LogRelationCreationInfo(ILogger logger, long relationId, int memberCount, int version, long changeSet, string user, DateTime timestamp, bool visible, string tags);

    }
}
