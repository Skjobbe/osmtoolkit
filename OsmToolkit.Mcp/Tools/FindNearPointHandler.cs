using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OsmToolkit.DataSources;
using OsmToolkit.Finders;
using OsmToolkit.Geocoding;
using OsmToolkit.Mcp.Tools.Logging;

namespace OsmToolkit.Mcp.Tools
{
    /// <summary>
    /// Application logic behind the <c>find_near_point</c> MCP tool: resolves a place name to a centroid,
    /// fetches OSM data covering the requested search radius, and returns the nearest nodes, optionally
    /// filtered by tags. Depends only on already-registered library interfaces, so it can be constructed
    /// and called directly in a test, without any MCP-specific transport or attribute involved.
    /// </summary>
    public class FindNearPointHandler
    {
        // Matches OsmToolkit.Finders.Spatial.GeoDistance's constants, duplicated here since that helper is
        // internal to the OsmToolkit assembly and not visible to this project.
        private const double MetersPerDegreeLatitude = 111_320d;
        private const double EarthRadiusMeters = 6_371_000d;
        private const double MaxAbsLatitudeDegrees = 89.9d;

        private readonly IPlaceLookup _placeLookup;
        private readonly IOsmDataSource _dataSource;
        private readonly INearestNodesFinder _nearestNodesFinder;
        private readonly IWithinDistanceFinder<OsmEntity> _withinDistanceFinder;
        private readonly ILogger<FindNearPointHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindNearPointHandler"/> class.
        /// </summary>
        /// <param name="placeLookup">Resolves the free-text place name to a geographic centroid.</param>
        /// <param name="dataSource">Fetches OSM data for the area covering the search radius.</param>
        /// <param name="nearestNodesFinder">Finds the nearest node(s) to a coordinate.</param>
        /// <param name="withinDistanceFinder">Narrows the fetched data down to entities within the search radius.</param>
        /// <param name="logger">An optional logger for diagnostics. If not provided, a <see cref="NullLogger{FindNearPointHandler}"/> is used.</param>
        public FindNearPointHandler(
            IPlaceLookup placeLookup,
            IOsmDataSource dataSource,
            INearestNodesFinder nearestNodesFinder,
            IWithinDistanceFinder<OsmEntity> withinDistanceFinder,
            ILogger<FindNearPointHandler>? logger = null)
        {
            _placeLookup = placeLookup;
            _dataSource = dataSource;
            _nearestNodesFinder = nearestNodesFinder;
            _withinDistanceFinder = withinDistanceFinder;
            _logger = logger ?? new NullLogger<FindNearPointHandler>();
        }

        /// <summary>
        /// Finds the <see cref="Node"/> instances nearest to <paramref name="place"/>, within <paramref name="radiusMeters"/>.
        /// </summary>
        /// <param name="place">A free-text place name, e.g. a city, address, or landmark.</param>
        /// <param name="radiusMeters">The search radius around the place's centroid, in meters. Must be greater than zero.</param>
        /// <param name="tags">Optional tag filters to match, as exact key-value pairs. If <c>null</c> or empty, any node is eligible.</param>
        /// <param name="limit">The maximum number of nodes to return, ordered by distance from the place. Must be greater than zero.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>The nearest matching nodes, ordered by distance, with their tags and coordinates.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="place"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="radiusMeters"/> or <paramref name="limit"/> is not greater than zero.</exception>
        /// <exception cref="PlaceNotFoundException">Thrown when no place matches <paramref name="place"/>.</exception>
        public async Task<IReadOnlyList<NearPointMatch>> FindAsync(
            string place,
            double radiusMeters,
            IReadOnlyDictionary<string, string>? tags,
            int limit,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(place))
                throw new ArgumentException("Place cannot be null or empty.", nameof(place));

            if (radiusMeters <= 0)
                throw new ArgumentOutOfRangeException(nameof(radiusMeters), radiusMeters, "Radius must be greater than zero.");

            if (limit <= 0)
                throw new ArgumentOutOfRangeException(nameof(limit), limit, "Limit must be greater than zero.");

            FindNearPointLogMessages.LogSearchStart(_logger, place, radiusMeters, limit);

            var location = await _placeLookup.FindAsync(place, cancellationToken);
            var bounds = BoundsFromRadius(location.Latitude, location.Longitude, radiusMeters);
            var data = await _dataSource.GetOsmDataAsync(bounds, cancellationToken);
            var withinRadius = _withinDistanceFinder.FindNearByRadius(data, location.Latitude, location.Longitude, radiusMeters);

            var tagFilter = tags is { Count: > 0 } ? tags.ToDictionary(kv => kv.Key, kv => kv.Value) : null;

            var nodes = limit == 1
                ? FindNearestAsList(withinRadius, location.Latitude, location.Longitude, tagFilter)
                : FindNearbyNodes(withinRadius, location.Latitude, location.Longitude, limit, tagFilter);

            var matches = nodes
                .Select(node => new NearPointMatch(
                    node.Id,
                    node.Tags,
                    node.Latitude,
                    node.Longitude,
                    HaversineMeters(location.Latitude, location.Longitude, node.Latitude, node.Longitude)))
                .ToList();

            FindNearPointLogMessages.LogSearchResult(_logger, place, matches.Count);

            return matches;
        }

        private IReadOnlyList<Node> FindNearestAsList(OsmData data, double latitude, double longitude, Dictionary<string, string>? tags)
        {
            var nearest = tags is null
                ? _nearestNodesFinder.FindNearestNode(data, latitude, longitude)
                : _nearestNodesFinder.FindNearestNode(data, latitude, longitude, tags);

            return nearest is null ? Array.Empty<Node>() : new[] { nearest };
        }

        private IReadOnlyList<Node> FindNearbyNodes(OsmData data, double latitude, double longitude, int limit, Dictionary<string, string>? tags) =>
            tags is null
                ? _nearestNodesFinder.FindNearbyNodes(data, latitude, longitude, limit)
                : _nearestNodesFinder.FindNearbyNodes(data, latitude, longitude, limit, tags);

        /// <summary>
        /// Builds a bounding box covering a circle of <paramref name="radiusMeters"/> around the given centroid,
        /// suitable for fetching enough data via <see cref="IOsmDataSource"/> to then narrow down with
        /// <see cref="IWithinDistanceFinder{T}.FindNearByRadius(OsmData, double, double, double)"/>.
        /// </summary>
        private static OsmCoordinateBounds BoundsFromRadius(double latitude, double longitude, double radiusMeters)
        {
            var latitudeDelta = radiusMeters / MetersPerDegreeLatitude;
            var clampedLatitude = Math.Clamp(latitude, -MaxAbsLatitudeDegrees, MaxAbsLatitudeDegrees);
            var longitudeDelta = radiusMeters / (MetersPerDegreeLatitude * Math.Cos(clampedLatitude * Math.PI / 180d));

            return new OsmCoordinateBounds(
                Math.Max(latitude - latitudeDelta, -90d),
                Math.Max(longitude - longitudeDelta, -180d),
                Math.Min(latitude + latitudeDelta, 90d),
                Math.Min(longitude + longitudeDelta, 180d));
        }

        private static double HaversineMeters(double latitudeFrom, double longitudeFrom, double latitudeTo, double longitudeTo)
        {
            var latitudeRadiansFrom = latitudeFrom * Math.PI / 180d;
            var latitudeRadiansTo = latitudeTo * Math.PI / 180d;
            var latitudeDifference = (latitudeTo - latitudeFrom) * Math.PI / 180d;
            var longitudeDifference = (longitudeTo - longitudeFrom) * Math.PI / 180d;

            var a = Math.Sin(latitudeDifference / 2) * Math.Sin(latitudeDifference / 2) +
                    Math.Cos(latitudeRadiansFrom) * Math.Cos(latitudeRadiansTo) *
                    Math.Sin(longitudeDifference / 2) * Math.Sin(longitudeDifference / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusMeters * c;
        }
    }
}
