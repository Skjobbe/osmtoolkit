namespace OsmToolkit.DataSources
{
    /// <summary>
    /// Represents a data source that fetches OSM data for a geographic area from an external service,
    /// as opposed to an <see cref="Serialization.IOsmDeserializer"/> which only parses data already available locally.
    /// </summary>
    public interface IOsmDataSource
    {
        /// <summary>
        /// Asynchronously fetches OSM data for the specified bounds.
        /// </summary>
        /// <param name="bounds">The <see cref="OsmCoordinateBounds"/> describing the area to fetch data for.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An <see cref="OsmData"/> instance containing the nodes, ways and relations found within <paramref name="bounds"/>.</returns>
        Task<OsmData> GetOsmDataAsync(OsmCoordinateBounds bounds, CancellationToken cancellationToken = default);
    }
}
