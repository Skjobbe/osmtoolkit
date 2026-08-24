using Microsoft.Extensions.Logging;
using System.Net;

namespace OsmToolkit.Geocoding.Logging
{
    internal static partial class GeocodingLogMessages
    {
        [LoggerMessage(LogLevel.Debug, Message = "Looking up place \"{PlaceName}\" from {Endpoint}.")]
        internal static partial void LogFetchStart(ILogger logger, string placeName, string endpoint);

        [LoggerMessage(LogLevel.Debug, Message = "Served place lookup for \"{PlaceName}\" from cache.")]
        internal static partial void LogCacheHit(ILogger logger, string placeName);

        [LoggerMessage(LogLevel.Debug, Message = "Resolved \"{PlaceName}\" to ({Latitude},{Longitude}).")]
        internal static partial void LogFetchResult(ILogger logger, string placeName, double latitude, double longitude);

        [LoggerMessage(LogLevel.Error, Message = "Nominatim request failed with status code {StatusCode}.")]
        internal static partial void LogFetchFailed(ILogger logger, HttpStatusCode statusCode);

        [LoggerMessage(LogLevel.Warning, Message = "No place found matching \"{PlaceName}\".")]
        internal static partial void LogNoMatch(ILogger logger, string placeName);
    }
}
