using Microsoft.Extensions.Logging;

namespace OsmToolkit.Serialization.Logging
{
    internal static partial class SerializationLogMessages
    {
        [LoggerMessage(LogLevel.Information, Message = "OSM data serialized to string. {Result}")]
        internal static partial void LogSerializeAsync(ILogger logger, string result);

        [LoggerMessage(LogLevel.Information, Message = "OSM data saved at {path}.")]
        internal static partial void LogSerializeToFileAsync(ILogger logger, string path);

        [LoggerMessage(LogLevel.Information, Message = "OSM data saved to your stream.")]
        internal static partial void LogSerializeToStreamAsync(ILogger logger);

        [LoggerMessage(LogLevel.Error, Message = "An unexpected error occured during serialization of Osm data. {Ex}")]
        internal static partial void LogUnexpectedSerializationError(ILogger logger, string ex);

        [LoggerMessage(LogLevel.Debug, Message = "Osm data deserialized into {NodeCount} nodes, {WayCount} ways, {RelationCount} relations.")]
        internal static partial void LogDeserializeAsync(ILogger logger, int nodeCount, int wayCount, int relationCount);
    }
}
