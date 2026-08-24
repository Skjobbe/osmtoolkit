namespace OsmToolkit.Geocoding
{
    /// <summary>
    /// Resolves a free-text place name to a geographic location, as opposed to an
    /// <see cref="DataSources.IOsmDataSource"/>, which fetches OSM data for an already-known area.
    /// </summary>
    public interface IPlaceLookup
    {
        /// <summary>
        /// Asynchronously resolves <paramref name="placeName"/> to a geographic location.
        /// </summary>
        /// <param name="placeName">A free-text place name, e.g. a city, address, or landmark.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A <see cref="PlaceLookupResult"/> describing the best-matching place.</returns>
        /// <exception cref="PlaceNotFoundException">Thrown when no place matches <paramref name="placeName"/>.</exception>
        Task<PlaceLookupResult> FindAsync(string placeName, CancellationToken cancellationToken = default);
    }
}
