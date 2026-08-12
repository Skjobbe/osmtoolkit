using Microsoft.Extensions.Logging;

namespace OsmToolkit.Models.Logging
{
    internal static class EntityLoggingHelper
    {
        public static void LogSummaries(ILogger logger, OsmData data) => LogSummaries(logger, data.Nodes, data.Ways, data.Relations);

        public static void LogSummaries(ILogger logger, IList<Node> nodes, IList<Way> ways, IList<Relation> relations)
        {
            foreach (var node in nodes)
            {
                NodeLogMessages.LogNodeSummary(logger, node.Id, node.Latitude, node.Longitude, node.ToTagString());
            }

            foreach (var way in ways)
            {
                WayLogMessages.LogWaySummary(logger, way.Id, way.NodeReferenceIds.Count, way.ToTagString());
            }

            foreach (var relation in relations)
            {
                RelationLogMessages.LogRelationSummary(logger, relation.Id, relation.Members.Count, relation.ToTagString());
            }
        }

        public static void LogSummariesTagless(ILogger logger, OsmData data) => LogSummariesTagless(logger, data.Nodes, data.Ways, data.Relations);

        public static void LogSummariesTagless(ILogger logger, IList<Node> nodes, IList<Way> ways, IList<Relation> relations)
        {
            foreach (var node in nodes)
            {
                NodeLogMessages.LogNodeSummaryTagless(logger, node.Id, node.Latitude, node.Longitude);
            }

            foreach (var way in ways)
            {
                WayLogMessages.LogWaySummaryTagless(logger, way.Id, way.NodeReferenceIds.Count);
            }

            foreach (var relation in relations)
            {
                RelationLogMessages.LogRelationSummaryTagless(logger, relation.Id, relation.Members.Count);
            }
        }
            

    }
}
