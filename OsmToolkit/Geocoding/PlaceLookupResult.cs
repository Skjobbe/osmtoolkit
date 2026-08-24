namespace OsmToolkit.Geocoding
{
    /// <summary>
    /// The result of resolving a free-text place name to a geographic location.
    /// </summary>
    public class PlaceLookupResult
    {
        /// <summary>
        /// The human-readable name of the resolved place, as returned by the geocoding service.
        /// </summary>
        public string DisplayName { get; }
        /// <summary>
        /// The latitude of the place's centroid.
        /// </summary>
        public double Latitude { get; }
        /// <summary>
        /// The longitude of the place's centroid.
        /// </summary>
        public double Longitude { get; }
        /// <summary>
        /// The geographic area covering the place, suitable for passing straight into <see cref="DataSources.IOsmDataSource.GetOsmDataAsync"/>.
        /// </summary>
        public OsmCoordinateBounds Bounds { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceLookupResult"/> class.
        /// </summary>
        /// <param name="displayName">The human-readable name of the resolved place.</param>
        /// <param name="latitude">The latitude of the place's centroid.</param>
        /// <param name="longitude">The longitude of the place's centroid.</param>
        /// <param name="bounds">The geographic area covering the place.</param>
        public PlaceLookupResult(string displayName, double latitude, double longitude, OsmCoordinateBounds bounds)
        {
            DisplayName = displayName;
            Latitude = latitude;
            Longitude = longitude;
            Bounds = bounds;
        }
    }
}
