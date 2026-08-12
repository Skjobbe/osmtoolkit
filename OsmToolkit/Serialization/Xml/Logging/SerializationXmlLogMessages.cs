using Microsoft.Extensions.Logging;

namespace OsmToolkit.Serialization.Xml.Logging
{
    internal static partial class SerializationXmlLogMessages
    {
        internal static void LogWritingEntityStarted(ILogger logger, ReferenceType type, long id)
        {
            LogWritingEntityStartedGenerated(logger, type.ToString(), id);
        }
        internal static void LogWritingEntityFinished(ILogger logger, ReferenceType type, long id)
        {
            LogWritingEntityFinishedGenerated(logger, type.ToString(), id);
        }

        [LoggerMessage(LogLevel.Trace, Message = "Started writing {Entity} '{Id}'")]
        private static partial void LogWritingEntityStartedGenerated(ILogger logger, string entity, long id);
        [LoggerMessage(LogLevel.Trace, Message = "Successfully written {Entity} '{Id}'")]
        private static partial void LogWritingEntityFinishedGenerated(ILogger logger, string entity, long id);


        internal static void LogParsedEntity(ILogger logger, ReferenceType type, long id)
        {
            LogParsedEntityFinished(logger, type.ToString(), id);
        }

        [LoggerMessage(LogLevel.Trace, Message = "Parsed {Entity} '{Id}'")]
        private static partial void LogParsedEntityFinished(ILogger logger, string entity, long id);
        [LoggerMessage(LogLevel.Trace, Message = "Parsed bounds min=({MinLat}, {MinLon}), max=({maxLat}, {maxLon})")]
        private static partial void LogParsedBounds(ILogger logger, long minLat, long minLon, long maxLat, long maxLon);
        [LoggerMessage(LogLevel.Trace, Message = "Parsed header; version={Version}, generator={Generator}, copyright={Copyright}, attributionUrl={AttrUrl}, licenseUrl={LicenseUrl}")]
        private static partial void LogParsedHeader(ILogger logger, double version, string generator, string copyright, string attrUrl, string licenseUrl);
    }
}
