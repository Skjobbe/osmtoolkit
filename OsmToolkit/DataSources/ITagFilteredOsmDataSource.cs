namespace OsmToolkit.DataSources
{
    /// <summary>
    /// Represents a data source that fetches OSM data for a geographic area from an external service,
    /// restricted to entities matching a set of tag filters, as opposed to <see cref="IOsmDataSource"/>
    /// which fetches every entity within the bounds.
    /// </summary>
    public interface ITagFilteredOsmDataSource
    {
        /// <summary>
        /// Asynchronously fetches OSM data for the specified bounds, restricted to nodes, ways, and relations
        /// matching <paramref name="tags"/>. A matched way's or relation's member nodes are still returned even
        /// though they may not themselves carry a matching tag.
        /// </summary>
        /// <param name="bounds">The <see cref="OsmCoordinateBounds"/> describing the area to fetch data for.</param>
        /// <param name="tags">
        /// The tag filter to apply. Each key/value pair is ANDed together: an entity must carry every key. A
        /// <c>null</c> value means any value for that key is accepted; a non-<c>null</c> value requires an exact match.
        /// </param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An <see cref="OsmData"/> instance containing the nodes, ways and relations matching <paramref name="tags"/> within <paramref name="bounds"/>.</returns>
        Task<OsmData> GetOsmDataAsync(OsmCoordinateBounds bounds, IReadOnlyDictionary<string, string?> tags, CancellationToken cancellationToken = default);
    }
}
