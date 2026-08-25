using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OsmToolkit.DataSources;
using OsmToolkit.Finders;
using OsmToolkit.Geocoding;
using OsmToolkit.Mcp.Tools.Logging;

namespace OsmToolkit.Mcp.Tools
{
    /// <summary>
    /// Application logic behind the <c>route_between_points</c> MCP tool: resolves two place names to
    /// coordinates, fetches OSM data covering both, and calculates the shortest route between them for a
    /// requested travel mode. Depends only on already-registered library interfaces, so it can be
    /// constructed and called directly in a test, without any MCP-specific transport or attribute involved.
    /// </summary>
    public class RouteBetweenPointsHandler
    {
        // A route running due north/south or due east/west leaves origin and destination sharing a
        // latitude or longitude, which would otherwise produce a zero-width/zero-height bounding box -
        // OsmCoordinateBounds' constructor requires a strictly positive span on both axes.
        private const double BoundsPaddingDegrees = 0.001d;

        private static readonly IReadOnlyDictionary<string, TravelMode> TravelModesByName = new Dictionary<string, TravelMode>(StringComparer.OrdinalIgnoreCase)
        {
            ["foot"] = TravelMode.Foot,
            ["bicycle"] = TravelMode.Bicycle,
            ["moped"] = TravelMode.Moped,
            ["car"] = TravelMode.MotorCar,
        };

        private readonly IPlaceLookup _placeLookup;
        private readonly IOsmDataSource _dataSource;
        private readonly IShortestPathFinder _shortestPathFinder;
        private readonly ILogger<RouteBetweenPointsHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteBetweenPointsHandler"/> class.
        /// </summary>
        /// <param name="placeLookup">Resolves the free-text origin/destination place names to coordinates.</param>
        /// <param name="dataSource">Fetches OSM data for the area spanning both resolved places.</param>
        /// <param name="shortestPathFinder">Calculates the shortest route between the resolved coordinates.</param>
        /// <param name="logger">An optional logger for diagnostics. If not provided, a <see cref="NullLogger{RouteBetweenPointsHandler}"/> is used.</param>
        public RouteBetweenPointsHandler(
            IPlaceLookup placeLookup,
            IOsmDataSource dataSource,
            IShortestPathFinder shortestPathFinder,
            ILogger<RouteBetweenPointsHandler>? logger = null)
        {
            _placeLookup = placeLookup;
            _dataSource = dataSource;
            _shortestPathFinder = shortestPathFinder;
            _logger = logger ?? new NullLogger<RouteBetweenPointsHandler>();
        }

        /// <summary>
        /// Calculates the shortest route from <paramref name="origin"/> to <paramref name="destination"/>.
        /// </summary>
        /// <param name="origin">A free-text place name to start from, e.g. a city, address, or landmark.</param>
        /// <param name="destination">A free-text place name to route to.</param>
        /// <param name="travelMode">The mode of travel: <c>foot</c>, <c>bicycle</c>, <c>moped</c>, or <c>car</c>.</param>
        /// <param name="avoidMotorway">Whether to exclude motorways from the route.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>The calculated route, or a result with no waypoints and a <see cref="RouteResult.Description"/> explaining why none was found.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="origin"/> or <paramref name="destination"/> is null or empty, or <paramref name="travelMode"/> is not a recognized value.</exception>
        /// <exception cref="PlaceNotFoundException">Thrown when no place matches <paramref name="origin"/> or <paramref name="destination"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the area spanning <paramref name="origin"/> and <paramref name="destination"/> exceeds <see cref="OverpassOsmDataSource"/>'s area guardrail.</exception>
        public async Task<RouteResult> RouteAsync(
            string origin,
            string destination,
            string travelMode,
            bool avoidMotorway,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(origin))
                throw new ArgumentException("Origin cannot be null or empty.", nameof(origin));

            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException("Destination cannot be null or empty.", nameof(destination));

            var mode = ParseTravelMode(travelMode);
            var normalizedTravelMode = travelMode.Trim().ToLowerInvariant();

            RouteBetweenPointsLogMessages.LogRouteStart(_logger, origin, destination, normalizedTravelMode, avoidMotorway);

            var originLocationTask = _placeLookup.FindAsync(origin, cancellationToken);
            var destinationLocationTask = _placeLookup.FindAsync(destination, cancellationToken);
            await Task.WhenAll(originLocationTask, destinationLocationTask);

            var originLocation = originLocationTask.Result;
            var destinationLocation = destinationLocationTask.Result;

            var bounds = BoundsSpanning(originLocation, destinationLocation);
            var data = await _dataSource.GetOsmDataAsync(bounds, cancellationToken);

            var pathOptions = new PathOptions(mode, avoidMotorway);
            var path = _shortestPathFinder.FindShortestPath(
                data,
                originLocation.Latitude, originLocation.Longitude,
                destinationLocation.Latitude, destinationLocation.Longitude,
                pathOptions);

            var waypoints = path.Data.Nodes
                .Select(node => new RouteWaypoint(node.Latitude, node.Longitude))
                .ToList();

            RouteBetweenPointsLogMessages.LogRouteResult(_logger, origin, destination, path.TotalDistance, waypoints.Count);

            return new RouteResult(
                originLocation.DisplayName,
                destinationLocation.DisplayName,
                normalizedTravelMode,
                avoidMotorway,
                path.TotalDistance,
                waypoints,
                path.Description);
        }

        private static TravelMode ParseTravelMode(string travelMode)
        {
            if (string.IsNullOrWhiteSpace(travelMode) || !TravelModesByName.TryGetValue(travelMode.Trim(), out var mode))
                throw new ArgumentException($"Unknown travel mode \"{travelMode}\". Expected one of: foot, bicycle, moped, car.", nameof(travelMode));

            return mode;
        }

        /// <summary>
        /// Builds a bounding box spanning both resolved places' centroids, suitable for fetching enough data
        /// via <see cref="IOsmDataSource"/> to route between them - inheriting <see cref="IOsmDataSource"/>'s
        /// existing area guardrail, so an unreasonably large request fails fast with that guardrail's
        /// existing exception rather than a new error path.
        /// </summary>
        private static OsmCoordinateBounds BoundsSpanning(PlaceLookupResult origin, PlaceLookupResult destination)
        {
            var minLatitude = Math.Min(origin.Latitude, destination.Latitude);
            var maxLatitude = Math.Max(origin.Latitude, destination.Latitude);
            var minLongitude = Math.Min(origin.Longitude, destination.Longitude);
            var maxLongitude = Math.Max(origin.Longitude, destination.Longitude);

            return new OsmCoordinateBounds(
                Math.Max(minLatitude - BoundsPaddingDegrees, -90d),
                Math.Max(minLongitude - BoundsPaddingDegrees, -180d),
                Math.Min(maxLatitude + BoundsPaddingDegrees, 90d),
                Math.Min(maxLongitude + BoundsPaddingDegrees, 180d));
        }
    }
}
