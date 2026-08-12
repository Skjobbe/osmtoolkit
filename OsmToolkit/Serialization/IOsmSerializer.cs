namespace OsmToolkit.Serialization
{
    /// <summary>
    /// Defines methods for serializing <see cref="OsmData"/> objects into OSM data source.
    /// </summary>
    public interface IOsmSerializer
    {
        /// <summary>
        /// Asynchronously serializes the given <see cref="OsmData"/> object to a string.
        /// </summary>
        /// <param name="data">The OSM data to serialize.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A string containing the serialized OSM data source.</returns>
        Task<string> SerializeAsync(OsmData data, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously serializes the given <see cref="OsmData"/> object and writes the data source to a file.
        /// </summary>
        /// <param name="data">The OSM data to serialize.</param>
        /// <param name="path">The path to the file to write to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        Task SerializeToFileAsync(OsmData data, string path, CancellationToken cancellationToken = default);
        /// <summary>
        /// Asynchronously serializes the given <see cref="OsmData"/> object and writes the data source to the provided stream.
        /// </summary>
        /// <param name="data">The OSM data to serialize.</param>
        /// <param name="stream">The stream to write the data source output to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        Task SerializeToStreamAsync(OsmData data, Stream stream, CancellationToken cancellationToken = default);
    }
}
