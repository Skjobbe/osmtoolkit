using Microsoft.Extensions.Logging;

namespace OsmToolkit.Finders.Logging
{
    internal static partial class FinderLogMessages
    {

        //[LoggerMessage(LogLevel.Information, Message = "Found {nodeCount} nodes, {wayCount} ways, {relationCount} relations closest to latitude: {latitude}, longitude: {longitude}.")]
        //internal static partial void LogFindNearest(ILogger logger, int nodeCount, int wayCount, int relationCount, double latitude, double longitude);




        [LoggerMessage(LogLevel.Debug, Message = "FindByOsmId({Id}) result: {NodeCount} nodes, {WayCount} ways, {RelationCount} relations.")]
        internal static partial void LogFindByIdResults(ILogger logger, long id, int nodeCount, int wayCount, int relationCount);

        [LoggerMessage(LogLevel.Debug, Message = "FindByTag({Key}, {Value}) result: {NodeCount} nodes, {WayCount} ways, {RelationCount} relations.")]
        internal static partial void LogFindByTagResults(ILogger logger, string key, string value, int nodeCount, int wayCount, int relationCount);

        [LoggerMessage(LogLevel.Debug, Message = "FindByTags([{Tags}]) result: {NodeCount} nodes, {WayCount} ways, {RelationCount} relations.")]
        internal static partial void LogFindByTagsResults(ILogger logger, string tags, int nodeCount, int wayCount, int relationCount);

        [LoggerMessage(LogLevel.Debug, Message = "FindNearestNode({Latitude}, {Longitude}) result: Node {NodeId} at ({NodeLat}, {NodeLon}), distance: {Distance:F2} meters.")]
        internal static partial void LogFindNearestNodeResults(ILogger logger, double latitude, double longitude, long nodeId, double nodeLat, double nodeLon, double distance);

        [LoggerMessage(LogLevel.Debug, Message = "FindNearestNode({Latitude}, {Longitude}) failed: No node found.")]
        internal static partial void LogFindNearestNodeUnable(ILogger logger, double latitude, double longitude);


        [LoggerMessage(LogLevel.Debug, Message = "FindNearbyNodes({Latitude}, {Longitude}), limit={Limit}, tags={TagCount}, allowSameWay={AllowSameWay}, allowSameRelation={AllowSameRelation}.")]
        internal static partial void LogFindNearbyNodesStart(ILogger logger, double latitude, double longitude, int limit, int tagCount, bool allowSameWay, bool allowSameRelation);
        [LoggerMessage(LogLevel.Trace, Message = "Filtered by tags: [{Tags}]. Remaining nodes: {RemainingNodeCount}")]
        internal static partial void LogTagFilterResults(ILogger logger, string tags, int remainingNodeCount);

        [LoggerMessage(LogLevel.Trace, Message = "Node {Id} is {Distance} meters from target.")]
        internal static partial void LogDistanceOfNodesToTarget(ILogger logger, long id, double distance);

        [LoggerMessage(LogLevel.Debug, Message = "FindNearbyNodes() result: {Count} nearest nodes to ({Lat}, {Lon}): {Result}.")]
        internal static partial void LogFindNearbyNodesResult(ILogger logger, int count, double lat, double lon, string result);

        [LoggerMessage(LogLevel.Debug, Message = "FindNearByRadius({Latitude}, {Longitude}, radius={radiusMeters}) result: {nodeCount} nodes, {wayCount} ways, {relationCount} relations.")]
        internal static partial void LogFindNearByRadiusResults(ILogger logger, double latitude, double longitude, double radiusMeters, int nodeCount, int wayCount, int relationCount);


        [LoggerMessage(LogLevel.Debug, Message = "NearByPathDistance({Lat}, {Lon}, distance={DistanceMeters}) result: {nodeCount} nodes, {wayCount} ways, {relationCount} relations.")]
        internal static partial void LogFindNearByPathDistanceResults(ILogger logger, double lat, double lon, double distanceMeters, int nodeCount, int wayCount, int relationCount);


        [LoggerMessage(LogLevel.Debug, Message = "FindShortestPath(start=({StartLatitude}, {StartLongitude}), target=({TargetLatitude}, {TargetLongitude}), avoidMotorway={AvoidMotorway}, mode={Mode}) result: {nodeCount} nodes, {wayCount} ways, {relationCount} relations, total distance {TotalDistance}. Using start node {StartNodeId} and target node {TargetNodeId}")]
        internal static partial void LogFindShortestPathResult(ILogger logger, double startLatitude, double startLongitude, double targetLatitude, double targetLongitude, bool avoidMotorway, TravelMode mode, int nodeCount, int wayCount, int relationCount, double totalDistance, long startNodeId, long targetNodeId);
        


    }
}
