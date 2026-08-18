using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OsmToolkit.Serialization.IO;
using OsmToolkit.Serialization.Json.Dto;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using OsmToolkit.Serialization.Logging;

namespace OsmToolkit.Serialization.Json
{
    /// <summary>
    /// Provides functionality for deserializing OSM Json data into <see cref="OsmData"/> objects.
    /// </summary>
    internal class OsmJsonDeserializer : IOsmJsonDeserializer
    {
        private readonly ILogger<OsmJsonDeserializer> _logger;
        private readonly IFileProvider _fileProvider;
        /// <summary>
        /// Initializes a new instance of <see cref="OsmJsonDeserializer"/> class with optional logging and file access support.
        /// </summary>
        /// <param name="logger">An optional logger for diagnostics. If not provided, a <see cref="Microsoft.Extensions.Logging.Abstractions.NullLogger{OsmJsonDeserializer}"/> is used.</param>
        /// <param name="fileProvider">An optional file provider used to open input streams. If not provided, a default <see cref="FileProvider"/> implementation is used.</param>
        public OsmJsonDeserializer(ILogger<OsmJsonDeserializer>? logger = null, IFileProvider? fileProvider = null)
        {
            _logger = logger ?? new NullLogger<OsmJsonDeserializer>();
            _fileProvider = fileProvider ?? new FileProvider();
        }
        /// <inheritdoc />
        public async Task<OsmData> DeserializeAsync(string input, CancellationToken cancellationToken = default)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            using var stream = new MemoryStream(bytes);
            return await DeserializeWithReaderAsync(stream, cancellationToken);
        }
        /// <inheritdoc />
        public async Task<OsmData> DeserializeFromFileAsync(string path, CancellationToken cancellationToken = default)
        {
            var fileExtension = path.Split('.').Last();

            if (fileExtension != "json")
            {
                throw new ArgumentException($"{fileExtension} is invalid file format, must be'json'.", nameof(fileExtension));
            }

            await using var stream = await _fileProvider.OpenReadAsync(path, cancellationToken);
            return await DeserializeWithReaderAsync(stream, cancellationToken);
        }
        /// <inheritdoc />
        public async Task<OsmData> DeserializeFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            return await DeserializeWithReaderAsync(stream, cancellationToken);
        }

        private async Task<OsmData> DeserializeWithReaderAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return Deserialize(document);
        }

        /// <summary>
        /// Deserializes OSM JSON from an already-parsed <see cref="JsonDocument"/>, so a caller that has already
        /// parsed the response for another purpose (e.g. <see cref="OsmToolkit.DataSources.OverpassOsmDataSource"/>
        /// checking for an Overpass "remark" field) doesn't pay the cost of parsing it again.
        /// </summary>
        internal OsmData Deserialize(JsonDocument document)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var root = document.RootElement;
            bool isOverpass = root.TryGetProperty("elements", out _);

            if (isOverpass)
            {
                var overpass = root.Deserialize<OverpassJsonDto>(options)
                    ?? throw new InvalidDataException("Failed to deserialize Overpass JSON.");
                var resultData = OsmDataDto.FromOverpassJson(overpass);
                SerializationLogMessages.LogDeserializeAsync(_logger, resultData.Nodes.Count, resultData.Ways.Count, resultData.Relations.Count);
                return resultData;
            }
            else
            {
                var dto = root.Deserialize<OsmDataDto>(options)
                    ?? throw new InvalidDataException("Failed to deserialize JSON data.");

                var resultData = dto.FromJson();
                SerializationLogMessages.LogDeserializeAsync(_logger, resultData.Nodes.Count, resultData.Ways.Count, resultData.Relations.Count);
                return resultData;
            }
        }
    }
}
