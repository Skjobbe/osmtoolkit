using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OsmToolkit.Serialization.Logging;
using OsmToolkit.Serialization.Xml.Logging;
using System.Globalization;
using System.Xml;

namespace OsmToolkit.Serialization.Xml
{
    /// <summary>
    /// Provides functionality for serializing <see cref="OsmData"/> objects to OSM-compatible XML format.
    /// </summary>
    internal class OsmXmlSerializer : IOsmXmlSerializer
    {
        private readonly ILogger<OsmXmlSerializer> _logger;
        /// <summary>
        /// Initializes a new instance of the <see cref="OsmXmlSerializer"/> class with optional logging.
        /// </summary>
        /// <param name="logger">n optional logger for diagnostic or debug output. If null, a <see cref="NullLogger{T}"/> is used.</param>
        public OsmXmlSerializer(ILogger<OsmXmlSerializer>? logger = null)
        {
            _logger = logger ?? new NullLogger<OsmXmlSerializer>();
        }
        /// <inheritdoc />
        public async Task<string> SerializeAsync(OsmData data, CancellationToken cancellationToken = default)
        {
            using var stringWriter = new StringWriter();
            await SerializeWithWriterAsync(data, stringWriter, cancellationToken);
            var result = stringWriter.ToString();
            SerializationLogMessages.LogSerializeAsync(_logger, result);
            return result;
        }
        /// <inheritdoc />
        public async Task SerializeToFileAsync(OsmData data, string path, CancellationToken cancellationToken = default) 
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Write);
            using var writer = new StreamWriter(fileStream);
            await SerializeWithWriterAsync(data, writer, cancellationToken);
            SerializationLogMessages.LogSerializeToFileAsync(_logger, path);
        }
        /// <inheritdoc />
        public async Task SerializeToStreamAsync(OsmData data, Stream stream, CancellationToken cancellationToken = default)
        {
            using var writer = new StreamWriter(stream, leaveOpen: true);
            await SerializeWithWriterAsync(data, writer, cancellationToken);
            SerializationLogMessages.LogSerializeToStreamAsync(_logger);
        }

        private async Task SerializeWithWriterAsync(OsmData data, TextWriter writer, CancellationToken cancellationToken)
        {

            data.SortOsmData();
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Async = true
            };


            using var xmlWriter = XmlWriter.Create(writer, settings);

            await xmlWriter.WriteStartDocumentAsync();

            await WriteOsmHeader(data, xmlWriter);

            if(data.Bounds != null)
                await WriteOsmFileBounds(data, xmlWriter);

            foreach(var node in data.Nodes)
            {
                SerializationXmlLogMessages.LogWritingEntityStarted(_logger, ReferenceType.node, node.Id);
                cancellationToken.ThrowIfCancellationRequested();
                await WriteNode(node, xmlWriter);
                SerializationXmlLogMessages.LogWritingEntityFinished(_logger, ReferenceType.node, node.Id);
            }

            foreach(var way in data.Ways)
            {
                SerializationXmlLogMessages.LogWritingEntityStarted(_logger, ReferenceType.way, way.Id);
                cancellationToken.ThrowIfCancellationRequested();
                await WriteWay(way, xmlWriter);
                SerializationXmlLogMessages.LogWritingEntityFinished(_logger, ReferenceType.way, way.Id);
            }

            foreach(var relation in data.Relations)
            {
                SerializationXmlLogMessages.LogWritingEntityStarted(_logger, ReferenceType.relation, relation.Id);
                cancellationToken.ThrowIfCancellationRequested();
                await WriteRelation(relation, xmlWriter);
                SerializationXmlLogMessages.LogWritingEntityFinished(_logger, ReferenceType.relation, relation.Id);
            }


            await xmlWriter.WriteEndElementAsync(); // osmHeader
            await xmlWriter.WriteEndDocumentAsync();

            await writer.FlushAsync();
        }

        private static async Task WriteOsmHeader(OsmData data, XmlWriter xmlWriter)
        {
            await xmlWriter.WriteStartElementAsync(null, "osm", null);
            await xmlWriter.WriteAttributeStringAsync(null, "version", null, data.Header.Version.ToString(CultureInfo.InvariantCulture));
            await xmlWriter.WriteAttributeStringAsync(null, "generator", null, data.Header.Generator);
            await xmlWriter.WriteAttributeStringAsync(null, "copyright", null, data.Header.Copyright);
            await xmlWriter.WriteAttributeStringAsync(null, "attribution", null, data.Header.AttributionUrl);
            await xmlWriter.WriteAttributeStringAsync(null, "license", null, data.Header.LicenseUrl);
        }
        private static async Task WriteOsmFileBounds(OsmData data, XmlWriter xmlWriter)
        {
            await xmlWriter.WriteStartElementAsync(null, "bounds", null);
            await xmlWriter.WriteAttributeStringAsync(null, "minlat", null, data.Bounds!.MinimumLatitude.ToString(CultureInfo.InvariantCulture));
            await xmlWriter.WriteAttributeStringAsync(null, "minlon", null, data.Bounds!.MaximumLatitude.ToString(CultureInfo.InvariantCulture));
            await xmlWriter.WriteAttributeStringAsync(null, "maxlat", null, data.Bounds!.MinimumLongitude.ToString(CultureInfo.InvariantCulture));
            await xmlWriter.WriteAttributeStringAsync(null, "maxlon", null, data.Bounds!.MaximumLongitude.ToString(CultureInfo.InvariantCulture));
            await xmlWriter.WriteEndElementAsync();
        }
        private static async Task WriteNode(Node node, XmlWriter xmlWriter)
        {
            await xmlWriter.WriteStartElementAsync(null, "node", null);
            await xmlWriter.WriteAttributeStringAsync(null, "id", null, node.Id.ToString());
            await xmlWriter.WriteAttributeStringAsync(null, "visible", null, node.Visible.ToString().ToLowerInvariant());
            if(node.Version != -1)
                await xmlWriter.WriteAttributeStringAsync(null, "version", null, node.Version.ToString());
            if(node.ChangeSet != -1)
                await xmlWriter.WriteAttributeStringAsync(null, "changeset", null, node.ChangeSet.ToString());
            await xmlWriter.WriteAttributeStringAsync(null, "timestamp", null, node.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            if (node.User != null)
            {
                await xmlWriter.WriteAttributeStringAsync(null, "user", null, node.User.Name.ToString());
                await xmlWriter.WriteAttributeStringAsync(null, "uid", null, node.User.Id.ToString());
            }
            await xmlWriter.WriteAttributeStringAsync(null, "lat", null, node.Latitude.ToString(CultureInfo.InvariantCulture));
            await xmlWriter.WriteAttributeStringAsync(null, "lon", null, node.Longitude.ToString(CultureInfo.InvariantCulture));


            if (node.Tags.Count > 0)
            {
                foreach (var tag in node.Tags)
                {
                    await WriteTag(tag, xmlWriter);
                }
            }

            await xmlWriter.WriteEndElementAsync();
        }
        private static async Task WriteWay(Way way, XmlWriter xmlWriter)
        {
            await xmlWriter.WriteStartElementAsync(null, "way", null);
            await xmlWriter.WriteAttributeStringAsync(null, "id", null, way.Id.ToString());
            await xmlWriter.WriteAttributeStringAsync(null, "visible", null, way.Visible.ToString().ToLowerInvariant());
            if(way.Version != -1)
                await xmlWriter.WriteAttributeStringAsync(null, "version", null, way.Version.ToString());
            if(way.ChangeSet != -1)
                await xmlWriter.WriteAttributeStringAsync(null, "changeset", null, way.ChangeSet.ToString());
            await xmlWriter.WriteAttributeStringAsync(null, "timestamp", null, way.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            if (way.User != null)
            {
                await xmlWriter.WriteAttributeStringAsync(null, "user", null, way.User.Name.ToString());
                await xmlWriter.WriteAttributeStringAsync(null, "uid", null, way.User.Id.ToString());
            }

            foreach (var nodeIdRef in way.NodeReferenceIds)
            {
                await xmlWriter.WriteStartElementAsync(null, "nd", null);
                await xmlWriter.WriteAttributeStringAsync(null, "ref", null, nodeIdRef.ToString());
                await xmlWriter.WriteEndElementAsync();
            }

            foreach (var tag in way.Tags)
            {
                await WriteTag(tag, xmlWriter);
            }

            await xmlWriter.WriteEndElementAsync();
        }
        
        private static async Task WriteRelation(Relation relation,  XmlWriter xmlWriter)
        {
            await xmlWriter.WriteStartElementAsync(null, "relation", null);
            await xmlWriter.WriteAttributeStringAsync(null, "id", null, relation.Id.ToString());
            await xmlWriter.WriteAttributeStringAsync(null, "visible", null, relation.Visible.ToString().ToLowerInvariant());
            if(relation.Version != -1)
                await xmlWriter.WriteAttributeStringAsync(null, "version", null, relation.Version.ToString());
            if(relation.ChangeSet != -1)
                await xmlWriter.WriteAttributeStringAsync(null, "changeset", null, relation.ChangeSet.ToString());
            await xmlWriter.WriteAttributeStringAsync(null, "timestamp", null, relation.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            if (relation.User != null)
            {
                await xmlWriter.WriteAttributeStringAsync(null, "user", null, relation.User.Name.ToString());
                await xmlWriter.WriteAttributeStringAsync(null, "uid", null, relation.User.Id.ToString());
            }

            foreach(var member in relation.Members)
            {
                await WriteRelationMember(member, xmlWriter);
            }

            foreach(var tag in relation.Tags)
            {
                await WriteTag(tag, xmlWriter);
            }

            await xmlWriter.WriteEndElementAsync();
        }
        
        private static async Task WriteTag(KeyValuePair<string, string> tag, XmlWriter xmlWriter)
        {
            await xmlWriter.WriteStartElementAsync(null, "tag", null);
            await xmlWriter.WriteAttributeStringAsync(null, "k", null, tag.Key);
            await xmlWriter.WriteAttributeStringAsync(null, "v", null, tag.Value);
            await xmlWriter.WriteEndElementAsync();
        }
        private static async Task WriteRelationMember(Member member, XmlWriter xmlWriter)
        {
            await xmlWriter.WriteStartElementAsync(null, "member", null);
            await xmlWriter.WriteAttributeStringAsync(null, "type", null, member.Type.ToString());
            await xmlWriter.WriteAttributeStringAsync(null, "ref", null, member.ReferenceId.ToString());
            await xmlWriter.WriteAttributeStringAsync(null, "role", null, member.Role);
            await xmlWriter.WriteEndElementAsync();
        }
    }
}
