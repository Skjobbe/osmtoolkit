using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OsmToolkit.Serialization.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OsmToolkit.Serialization.Json
{
    /// <summary>
    /// Provides functionality for serializing <see cref="OsmData"/> objects to OSM-compatible JSON format.
    /// </summary>
    internal class OsmJsonSerializer : IOsmJsonSerializer
    {
        private readonly ILogger<OsmJsonSerializer> _logger;
        /// <summary>
        /// Initializes a new instance of the <see cref="OsmJsonSerializer"/> class with optional logging.
        /// </summary>
        /// <param name="logger">n optional logger for diagnostic or debug output. If null, a <see cref="NullLogger{T}"/> is used.</param>
        public OsmJsonSerializer(ILogger<OsmJsonSerializer>? logger = null)
        {
            _logger = logger ?? new NullLogger<OsmJsonSerializer>(); ;
        }
        /// <inheritdoc />
        public Task<string> SerializeAsync(OsmData data, CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };

                data.SortOsmData();

                string jsonString = JsonSerializer.Serialize(data, options);

                SerializationLogMessages.LogSerializeAsync(_logger, jsonString);
                return Task.FromResult(jsonString);
            }
            catch (Exception ex)
            {
                SerializationLogMessages.LogUnexpectedSerializationError(_logger, ex.Message);
                throw;
            }
        }
        /// <inheritdoc />
        public async Task SerializeToFileAsync(OsmData data, string path, CancellationToken cancellationToken = default)
        {
            try
            {
                data.SortOsmData();

                await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
                await SerializeToStreamAsync(data, stream, cancellationToken);
                SerializationLogMessages.LogSerializeToFileAsync(_logger, path);
            }
            catch (Exception ex)
            {
                SerializationLogMessages.LogUnexpectedSerializationError(_logger, ex.Message);
                throw;
            }
        }
        /// <inheritdoc />
        public async Task SerializeToStreamAsync(OsmData data, Stream stream, CancellationToken cancellationToken = default)
        {
            try
            {
                data.SortOsmData();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                await JsonSerializer.SerializeAsync(stream, data, options, cancellationToken);
                SerializationLogMessages.LogSerializeToStreamAsync(_logger);
            }
            catch (Exception ex)
            {
                SerializationLogMessages.LogUnexpectedSerializationError(_logger, ex.Message);
                throw;
            }
        }
    }
}
