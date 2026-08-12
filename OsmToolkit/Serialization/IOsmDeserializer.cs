namespace OsmToolkit.Serialization
{
    /// <summary>
    /// Defines methods for deserializing OSM data into <see cref="OsmData"/> objects.
    /// </summary>
    public interface IOsmDeserializer
    {
        /// <summary>
        /// Asynchronously deserializes OSM content from a string.
        /// </summary>
        /// <param name="input">A string containing valid OSM data source.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An <see cref="OsmData"/> object representing the deserialized OSM data.</returns>
        Task<OsmData> DeserializeAsync(string input, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously deserializes OSM data from a stream.
        /// </summary>
        /// <param name="stream">A readable <see cref="Stream"/> containing OSM data.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An <see cref="OsmData"/> object representing the deserialized OSM data.</returns>
        Task<OsmData> DeserializeFromStreamAsync(Stream stream, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously deserializes OSM data from the specified file path.
        /// </summary>
        /// <param name="path">The path to the file to deserialize.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An <see cref="OsmData"/> object representing the deserialized OSM data.</returns>
        /// <exception cref="ArgumentException">Thrown when the file extension is not 'osm' or 'xml'.</exception>
        Task<OsmData> DeserializeFromFileAsync(string path, CancellationToken cancellationToken = default);
    }
}
