using Microsoft.Extensions.Logging;
using System.Net;

namespace OsmToolkit.DataSources.Logging
{
    internal static partial class DataSourceLogMessages
    {
        [LoggerMessage(LogLevel.Debug, Message = "Fetching OSM data for bounds ({MinLat},{MinLon})-({MaxLat},{MaxLon}) from {Endpoint}.")]
        internal static partial void LogFetchStart(ILogger logger, double minLat, double minLon, double maxLat, double maxLon, string endpoint);

        [LoggerMessage(LogLevel.Debug, Message = "Fetched OSM data: {NodeCount} nodes, {WayCount} ways, {RelationCount} relations.")]
        internal static partial void LogFetchResult(ILogger logger, int nodeCount, int wayCount, int relationCount);

        [LoggerMessage(LogLevel.Error, Message = "Overpass request failed with status code {StatusCode}.")]
        internal static partial void LogFetchFailed(ILogger logger, HttpStatusCode statusCode);

        [LoggerMessage(LogLevel.Warning, Message = "Rejected request: estimated area {AreaSquareKilometers} km² exceeds maximum allowed area {MaxAreaSquareKilometers} km².")]
        internal static partial void LogAreaRejected(ILogger logger, double areaSquareKilometers, double maxAreaSquareKilometers);

        [LoggerMessage(LogLevel.Error, Message = "Overpass query failed server-side: {Remark}")]
        internal static partial void LogRemarkDetected(ILogger logger, string remark);
    }
}
