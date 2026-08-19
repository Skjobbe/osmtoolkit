namespace OsmToolkit.Finders.Spatial
{
    /// <summary>
    /// A uniform grid-based spatial index over a fixed collection of <see cref="Node"/> instances, used by
    /// <see cref="OsmEntityFinder"/> to answer proximity queries without a linear scan over every node.
    /// Cells are sized in meters; the longitude dimension is corrected for latitude-dependent shrinkage
    /// via <see cref="GeoDistance.MetersPerDegreeLongitude"/>. Built once and treated as immutable afterwards -
    /// callers are expected to cache and reuse an instance for the lifetime of the <see cref="Node"/> collection
    /// it was built from.
    /// </summary>
    internal sealed class GridNodeIndex
    {
        /// <summary>
        /// Default grid cell size, in meters. Not user-configurable - purely an internal tuning knob for the
        /// coarse pre-filter's granularity; correctness does not depend on its value.
        /// </summary>
        internal const double DefaultCellSizeMeters = 500d;

        private readonly double _latitudeCellSizeDegrees;
        private readonly double _longitudeCellSizeDegrees;
        private readonly Dictionary<(long Row, long Column), List<Node>> _cells;

        private GridNodeIndex(double latitudeCellSizeDegrees, double longitudeCellSizeDegrees, Dictionary<(long Row, long Column), List<Node>> cells)
        {
            _latitudeCellSizeDegrees = latitudeCellSizeDegrees;
            _longitudeCellSizeDegrees = longitudeCellSizeDegrees;
            _cells = cells;
        }

        /// <summary>
        /// Builds a <see cref="GridNodeIndex"/> over the given nodes.
        /// </summary>
        /// <param name="nodes">Nodes to index. May be empty.</param>
        /// <param name="cellSizeMeters">Grid cell size in meters, before longitude correction. Defaults to <see cref="DefaultCellSizeMeters"/>.</param>
        internal static GridNodeIndex Build(IEnumerable<Node> nodes, double cellSizeMeters = DefaultCellSizeMeters)
        {
            var nodeList = nodes as IReadOnlyCollection<Node> ?? nodes.ToList();

            // A single reference latitude - the mean of the indexed nodes - is used to size the longitude
            // dimension of every cell. Query-time range calculations correct for the query's own latitude
            // independently (see FindWithinRadius), so an imprecise reference latitude only affects how many
            // (mostly empty) cells get scanned, never correctness.
            double referenceLatitude = nodeList.Count > 0 ? nodeList.Average(n => n.Latitude) : 0d;

            double latitudeCellSizeDegrees = cellSizeMeters / GeoDistance.MetersPerDegreeLatitude;
            double longitudeCellSizeDegrees = cellSizeMeters / GeoDistance.MetersPerDegreeLongitude(referenceLatitude);

            var cells = new Dictionary<(long Row, long Column), List<Node>>();
            foreach (var node in nodeList)
            {
                var key = (CellIndex(node.Latitude, latitudeCellSizeDegrees), CellIndex(node.Longitude, longitudeCellSizeDegrees));
                if (!cells.TryGetValue(key, out var bucket))
                {
                    bucket = new List<Node>();
                    cells[key] = bucket;
                }

                bucket.Add(node);
            }

            return new GridNodeIndex(latitudeCellSizeDegrees, longitudeCellSizeDegrees, cells);
        }

        /// <summary>
        /// Returns every indexed <see cref="Node"/> within <paramref name="radiusMeters"/> of the given coordinate.
        /// Scans the grid cells overlapping a bounding box around the query point and radius as a coarse pre-filter,
        /// then applies an exact Haversine-distance check to that candidate set.
        /// </summary>
        internal List<Node> FindWithinRadius(double latitude, double longitude, double radiusMeters)
        {
            var results = new List<Node>();
            if (_cells.Count == 0)
                return results;

            double latitudeDegreeRadius = radiusMeters / GeoDistance.MetersPerDegreeLatitude;
            double longitudeDegreeRadius = radiusMeters / GeoDistance.MetersPerDegreeLongitude(latitude);

            long rowMin = CellIndex(latitude - latitudeDegreeRadius, _latitudeCellSizeDegrees);
            long rowMax = CellIndex(latitude + latitudeDegreeRadius, _latitudeCellSizeDegrees);
            long columnMin = CellIndex(longitude - longitudeDegreeRadius, _longitudeCellSizeDegrees);
            long columnMax = CellIndex(longitude + longitudeDegreeRadius, _longitudeCellSizeDegrees);

            for (long row = rowMin; row <= rowMax; row++)
            {
                for (long column = columnMin; column <= columnMax; column++)
                {
                    if (!_cells.TryGetValue((row, column), out var bucket))
                        continue;

                    foreach (var node in bucket)
                    {
                        if (GeoDistance.HaversineMeters(latitude, longitude, node.Latitude, node.Longitude) <= radiusMeters)
                            results.Add(node);
                    }
                }
            }

            return results;
        }

        private static long CellIndex(double value, double cellSizeDegrees) => (long)Math.Floor(value / cellSizeDegrees);
    }
}
