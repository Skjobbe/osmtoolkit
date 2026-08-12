using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OsmToolkit.Serialization.IO;
using OsmToolkit.Serialization.Logging;
using OsmToolkit.Serialization.Xml.Logging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Xml;

[assembly: InternalsVisibleTo("OsmToolkitTests")]

namespace OsmToolkit.Serialization.Xml
{
    /// <summary>
    /// Provides functionality for deserializing OSM XML data into <see cref="OsmData"/> objects.
    /// </summary>
    internal class OsmXmlDeserializer : IOsmXmlDeserializer
    {
        private readonly ILogger<OsmXmlDeserializer> _logger;
        private readonly IFileProvider _fileProvider;
        /// <summary>
        /// Initializes a new instance of <see cref="OsmXmlDeserializer"/> class with optional logging and file access support.
        /// </summary>
        /// <param name="logger">An optional logger for diagnostics. If not provided, a <see cref="Microsoft.Extensions.Logging.Abstractions.NullLogger{OsmXmlDeserializer}"/> is used.</param>
        /// <param name="fileProvider">An optional file provider used to open input streams. If not provided, a default <see cref="FileProvider"/> implementation is used.</param>
        public OsmXmlDeserializer(ILogger<OsmXmlDeserializer>? logger = null, IFileProvider? fileProvider = null)
        {
            _logger = logger ?? new NullLogger<OsmXmlDeserializer>();
            _fileProvider = fileProvider ?? new FileProvider();
        }
        /// <inheritdoc />
        public async Task<OsmData> DeserializeAsync(string input, CancellationToken cancellationToken = default)
        {
            using var stringReader = new StringReader(input);
            return await DeserializeWithReaderAsync(stringReader, cancellationToken);
        }
        /// <inheritdoc />
        public async Task<OsmData> DeserializeFromFileAsync(string path, CancellationToken cancellationToken = default)
        {
            var fileExtension = path.Split('.').Last();

            if (fileExtension != "osm" && fileExtension != "xml")
            {
                throw new ArgumentException($"{fileExtension} is invalid file format, must be 'osm' or 'xml'.", nameof(fileExtension));
            }

            await using var stream = await _fileProvider.OpenReadAsync(path, cancellationToken);
            using var reader = new StreamReader(stream);
            return await DeserializeWithReaderAsync(reader, cancellationToken);
        }
        /// <inheritdoc />
        public async Task<OsmData> DeserializeFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            return await DeserializeWithReaderAsync(reader, cancellationToken);
        }

        private async Task<OsmData> DeserializeWithReaderAsync(TextReader reader, CancellationToken cancellationToken = default)
        {
            var nodes = new List<Node>();
            var ways = new List<Way>();
            var relations = new List<Relation>();

            OsmHeader? header = null;
            OsmCoordinateBounds? bounds = null;

            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings 
            {
                Async = true
            };

            using var xmlReader = XmlReader.Create(reader, xmlReaderSettings);

            while (await xmlReader.ReadAsync().ConfigureAwait(false))
            {
                if(xmlReader.NodeType == XmlNodeType.Element)
                {
                    switch(xmlReader.Name)
                    {
                        case "osm":
                            cancellationToken.ThrowIfCancellationRequested();
                            header = ParseOsmHeader(xmlReader);
                            break;

                        case "bounds":
                            cancellationToken.ThrowIfCancellationRequested();
                            bounds = ParseBounds(xmlReader);
                            break;

                        case "node":
                            cancellationToken.ThrowIfCancellationRequested();
                            var node = await ParseNodeAsync(xmlReader, cancellationToken);
                            nodes.Add(node);
                            SerializationXmlLogMessages.LogParsedEntity(_logger, ReferenceType.node, node.Id);
                            break;

                        case "way":
                            cancellationToken.ThrowIfCancellationRequested();
                            var way = await ParseWayAsync(xmlReader, cancellationToken);
                            ways.Add(way);
                            SerializationXmlLogMessages.LogParsedEntity(_logger, ReferenceType.way, way.Id);
                            break;

                        case "relation":
                            cancellationToken.ThrowIfCancellationRequested();
                            var relation = await ParseRelationAsync(xmlReader, cancellationToken);
                            relations.Add(relation);
                            SerializationXmlLogMessages.LogParsedEntity(_logger, ReferenceType.relation, relation.Id);
                            break;
                        default:
                            break;
                    }
                }
            }

            if (header is null)
            {
                throw new InvalidOperationException("<osm> header element not found.");
            }

            SerializationLogMessages.LogDeserializeAsync(_logger, nodes.Count, ways.Count, relations.Count);
            return new OsmData(header, bounds, nodes, ways, relations);
        }

        private async Task<Relation> ParseRelationAsync(XmlReader xmlReader, CancellationToken cancellationToken)
        {
            var (id, visible, version, changeset, timestamp, user) = ParseCommonEntityValues(xmlReader);

            var members = new List<Member>();
            var tags = new Dictionary<string, string>();

            if (!xmlReader.IsEmptyElement)
            {
                while (await xmlReader.ReadAsync().ConfigureAwait(false))
                {
                    if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "relation")
                        break;

                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        switch (xmlReader.Name)
                        {
                            case "member":
                                cancellationToken.ThrowIfCancellationRequested();
                                var member = ParseMember(xmlReader);
                                members.Add(member);
                                break;

                            case "tag":
                                cancellationToken.ThrowIfCancellationRequested();
                                var tag = ParseTag(xmlReader);
                                tags.Add(tag.Key, tag.Value);
                                break;
                        }
                    }
                }
            }

            var relation = new Relation(id, visible, version, changeset, timestamp, user, members, tags);

            return relation;
        }

        private async Task<Way> ParseWayAsync(XmlReader xmlReader, CancellationToken cancellationToken)
        {
            var (id, visible, version, changeset, timestamp, user) = ParseCommonEntityValues(xmlReader);

            var nodeRefs = new List<long>();
            var tags = new Dictionary<string, string>();

            if (!xmlReader.IsEmptyElement)
            {
                while (await xmlReader.ReadAsync().ConfigureAwait(false))
                {
                    if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "way")
                        break;

                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        switch (xmlReader.Name)
                        {
                            case "nd":
                                cancellationToken.ThrowIfCancellationRequested();
                                var nodeRef = ParseNodeRef(xmlReader);
                                nodeRefs.Add(nodeRef);
                                break;

                            case "tag":
                                cancellationToken.ThrowIfCancellationRequested();
                                var tag = ParseTag(xmlReader);
                                tags.Add(tag.Key, tag.Value);
                                break;
                        }
                    }
                }
            }

            var Way = new Way(id, visible, version, changeset, timestamp, user, nodeRefs, tags);

            return Way;
        }

        private static long ParseNodeRef(XmlReader xmlReader)
        {
            var refId = ParseRequiredAttribute<long>(xmlReader, "ref");

            return refId;
        }

        private async Task<Node> ParseNodeAsync(XmlReader xmlReader, CancellationToken cancellationToken)
        {
            var (id, visible, version, changeset, timestamp, user) = ParseCommonEntityValues(xmlReader);

            var lon = ParseRequiredAttribute<double>(xmlReader, "lon");
            var lat = ParseRequiredAttribute<double>(xmlReader, "lat");
            var tags = new Dictionary<string, string>();

            if (!xmlReader.IsEmptyElement)
            {
                while (await xmlReader.ReadAsync().ConfigureAwait(false))
                {
                    if (xmlReader.NodeType == XmlNodeType.EndElement && xmlReader.Name == "node")
                        break;

                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        if (xmlReader.Name == "tag")
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var tag = ParseTag(xmlReader);
                            tags.Add(tag.Key, tag.Value);

                        }
                    }
                }
            }

            var node = new Node(id, visible, version, changeset, timestamp, user, lat, lon, tags);
            return node;
        }
        private static (long id, bool visible, int version, long changeset, DateTime timestamp, User user) ParseCommonEntityValues(XmlReader xmlReader)
        {
            var id = ParseRequiredAttribute<long>(xmlReader, "id");
            if(id == -1)
                throw new InvalidDataException($"Missing or invalid 'id' attribute in <{xmlReader.Name}>.");
            var visible = ParseRequiredAttribute<bool>(xmlReader, "visible");
            var version = ParseRequiredAttribute<int>(xmlReader, "version");
            long changeset = ParseRequiredAttribute<long>(xmlReader, "changeset");
            
            var timestamp = ParseRequiredAttribute<DateTime>(xmlReader, "timestamp");

            var userName = xmlReader.GetAttribute("user") ?? null;
            long uid = -1;
            if(userName != null)
                uid = ParseRequiredAttribute<long>(xmlReader, "uid");

            User? user = null;
            if (userName != null && uid != -1)
                user = new User(uid, userName);

            return (id, visible, version, changeset, timestamp, user!);

        }
        private static KeyValuePair<string, string> ParseTag(XmlReader xmlReader)
        {
            var key = xmlReader.GetAttribute("k");
            var value = xmlReader.GetAttribute("v") ?? string.Empty;

            if (string.IsNullOrEmpty(key))
                throw new InvalidDataException();

            return new KeyValuePair<string, string>(key, value);
        }

        private static Member ParseMember(XmlReader xmlReader)
        {
            ReferenceType type = ParseRequiredAttribute<ReferenceType>(xmlReader, "type");
            var refId = ParseRequiredAttribute<long>(xmlReader, "ref");
            var role = xmlReader.GetAttribute("role");

            return new Member(type, refId, role);
        }

        private static OsmHeader ParseOsmHeader(XmlReader xmlReader)
        {
            var version = ParseRequiredAttribute<double>(xmlReader, "version");
            if (version == -1)
                throw new InvalidDataException($"Missing or invalid 'version' attribute in <{xmlReader.Name}>.");

            var generator = xmlReader.GetAttribute("generator");
            var copyright = xmlReader.GetAttribute("copyright");
            var attribution = xmlReader.GetAttribute("attribution");
            var license = xmlReader.GetAttribute("license");

            return new OsmHeader(version, generator, copyright, attribution, license);
        }
        private static OsmCoordinateBounds ParseBounds(XmlReader xmlReader)
        {
            var minLat = ParseRequiredAttribute<double>(xmlReader, "minlat");
            var minLon = ParseRequiredAttribute<double>(xmlReader, "minlon");
            var maxLat = ParseRequiredAttribute<double>(xmlReader, "maxlat");
            var maxLon = ParseRequiredAttribute<double>(xmlReader, "maxlon");

            return new OsmCoordinateBounds(minLat, minLon, maxLat, maxLon);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlReader"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException">Thrown if missing attribute or if invalid format.</exception>
        /// <exception cref="NotSupportedException"></exception>
        private static T ParseRequiredAttribute<T>(XmlReader xmlReader, string attributeName)
        {

            var raw = xmlReader.GetAttribute(attributeName);
            var nodeName = xmlReader.Name;

            //if (string.IsNullOrWhiteSpace(raw))
            //    throw new InvalidDataException($"Missing or invalid '{attributeName}' attribute in <{nodeName}>.");

            return ParseValue<T>(raw, attributeName, nodeName);

        }
        private static T ParseValue<T>(string? raw, string attributeName, string nodeName)
        {
            try
            {
                if (typeof(T) == typeof(int))
                    return (T)(object)(string.IsNullOrEmpty(raw) ? -1 : int.Parse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture));

                if (typeof(T) == typeof(long))
                    return (T)(object)(string.IsNullOrEmpty(raw) ? -1 : long.Parse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture));

                if (typeof(T) == typeof(bool))
                    return (T)(object)(string.IsNullOrEmpty(raw) ? true : bool.Parse(raw));

                if (typeof(T) == typeof(decimal))
                    return (T)(object)(string.IsNullOrEmpty(raw) ? -1 : decimal.Parse(raw, NumberStyles.Float, CultureInfo.InvariantCulture));

                if (typeof(T) == typeof(double))
                    return (T)(object)(string.IsNullOrEmpty(raw) ? -1 : double.Parse(raw, NumberStyles.Float, CultureInfo.InvariantCulture));

                if (typeof(T) == typeof(DateTime))
                    return (T)(object)(string.IsNullOrEmpty(raw) ? DateTime.UtcNow : DateTime.Parse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
                if (typeof(T) == typeof(ReferenceType))
                    return (T)(object)Enum.Parse<ReferenceType>(raw!, true);
                    

                throw new NotSupportedException($"Type '{typeof(T).Name}' is not supported.");
            }
            catch (FormatException)
            {
                throw new InvalidDataException($"Invalid format for '{attributeName}' in <{nodeName}>.");
            }
        }
    }
}
