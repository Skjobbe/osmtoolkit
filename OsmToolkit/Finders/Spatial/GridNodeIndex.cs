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
        private readonly long _minRow;
        private readonly long _maxRow;
        private readonly long _minColumn;
        private readonly long _maxColumn;

        private GridNodeIndex(double latitudeCellSizeDegrees, double longitudeCellSizeDegrees, Dictionary<(long Row, long Column), List<Node>> cells)
        {
            _latitudeCellSizeDegrees = latitudeCellSizeDegrees;
            _longitudeCellSizeDegrees = longitudeCellSizeDegrees;
            _cells = cells;

            if (cells.Count > 0)
            {
                _minRow = cells.Keys.Min(key => key.Row);
                _maxRow = cells.Keys.Max(key => key.Row);
                _minColumn = cells.Keys.Min(key => key.Column);
                _maxColumn = cells.Keys.Max(key => key.Column);
            }
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

        /// <summary>
        /// Returns the single indexed <see cref="Node"/> closest to the given coordinate, or <c>null</c> if the
        /// index has no nodes. Searches grid cells in expanding square rings outward from the query point's own
        /// cell, stopping once the current best candidate is provably closer than any node an unsearched cell
        /// could contain - a single cell isn't sufficient, since a node in an adjacent cell can be closer than a
        /// node in the far corner of the query point's own cell.
        /// </summary>
        internal Node? FindNearest(double latitude, double longitude)
        {
            if (_cells.Count == 0)
                return null;

            long row = CellIndex(latitude, _latitudeCellSizeDegrees);
            long column = CellIndex(longitude, _longitudeCellSizeDegrees);

            long maxRing = Math.Max(
                Math.Max(Math.Abs(row - _minRow), Math.Abs(row - _maxRow)),
                Math.Max(Math.Abs(column - _minColumn), Math.Abs(column - _maxColumn)));

            Node? best = null;
            double bestDistanceMeters = double.PositiveInfinity;

            for (long ring = 0; ring <= maxRing; ring++)
            {
                foreach (var cellKey in CellsInRing(row, column, ring))
                {
                    if (!_cells.TryGetValue(cellKey, out var bucket))
                        continue;

                    foreach (var node in bucket)
                    {
                        double distance = GeoDistance.HaversineMeters(latitude, longitude, node.Latitude, node.Longitude);
                        if (distance < bestDistanceMeters)
                        {
                            bestDistanceMeters = distance;
                            best = node;
                        }
                    }
                }

                if (best != null && bestDistanceMeters <= SearchedAreaMarginMeters(latitude, longitude, row, column, ring))
                    break;
            }

            return best;
        }

        /// <summary>
        /// Returns the <paramref name="limit"/> indexed <see cref="Node"/> instances closest to the given
        /// coordinate, nearest first. Returns fewer than <paramref name="limit"/> if the index has fewer nodes.
        /// Searches grid cells in expanding square rings outward from the query point's own cell, stopping once
        /// the current <paramref name="limit"/>-th best candidate is provably closer than any node an unsearched
        /// cell could contain - the same guarantee <see cref="FindNearest(double, double)"/> uses for a single
        /// nearest node, generalized to the Nth-nearest.
        /// </summary>
        internal List<Node> FindNearest(double latitude, double longitude, int limit)
        {
            var candidates = new List<(Node Node, double DistanceMeters)>();
            if (_cells.Count == 0 || limit <= 0)
                return new List<Node>();

            long row = CellIndex(latitude, _latitudeCellSizeDegrees);
            long column = CellIndex(longitude, _longitudeCellSizeDegrees);

            long maxRing = Math.Max(
                Math.Max(Math.Abs(row - _minRow), Math.Abs(row - _maxRow)),
                Math.Max(Math.Abs(column - _minColumn), Math.Abs(column - _maxColumn)));

            for (long ring = 0; ring <= maxRing; ring++)
            {
                foreach (var cellKey in CellsInRing(row, column, ring))
                {
                    if (!_cells.TryGetValue(cellKey, out var bucket))
                        continue;

                    foreach (var node in bucket)
                        candidates.Add((node, GeoDistance.HaversineMeters(latitude, longitude, node.Latitude, node.Longitude)));
                }

                if (candidates.Count >= limit)
                {
                    candidates.Sort((a, b) => a.DistanceMeters.CompareTo(b.DistanceMeters));
                    if (candidates[limit - 1].DistanceMeters <= SearchedAreaMarginMeters(latitude, longitude, row, column, ring))
                        break;
                }
            }

            candidates.Sort((a, b) => a.DistanceMeters.CompareTo(b.DistanceMeters));
            return candidates.Take(limit).Select(c => c.Node).ToList();
        }

        /// <summary>
        /// The minimum possible real-world distance from the query point to any cell not yet covered by the
        /// square of rings searched so far (rings <c>0..ring</c> around the query point's own cell). Any node in
        /// an unsearched cell is at least this far away, so a candidate at or under this distance can never be
        /// beaten by expanding the search further.
        /// </summary>
        private double SearchedAreaMarginMeters(double latitude, double longitude, long row, long column, long ring)
        {
            double latitudeMin = (row - ring) * _latitudeCellSizeDegrees;
            double latitudeMax = (row + ring + 1) * _latitudeCellSizeDegrees;
            double longitudeMin = (column - ring) * _longitudeCellSizeDegrees;
            double longitudeMax = (column + ring + 1) * _longitudeCellSizeDegrees;

            double latitudeMarginDegrees = Math.Min(latitude - latitudeMin, latitudeMax - latitude);
            double longitudeMarginDegrees = Math.Min(longitude - longitudeMin, longitudeMax - longitude);

            double latitudeMarginMeters = latitudeMarginDegrees * GeoDistance.MetersPerDegreeLatitude;
            double longitudeMarginMeters = longitudeMarginDegrees * GeoDistance.MetersPerDegreeLongitude(latitude);

            return Math.Min(latitudeMarginMeters, longitudeMarginMeters);
        }

        /// <summary>
        /// Yields the cell coordinates on the border of the square ring at the given distance from
        /// <paramref name="row"/>/<paramref name="column"/> - just the center cell for ring 0, otherwise the
        /// outline of the <c>(2*ring+1)</c>-wide square, excluding cells already yielded by smaller rings.
        /// </summary>
        private static IEnumerable<(long Row, long Column)> CellsInRing(long row, long column, long ring)
        {
            if (ring == 0)
            {
                yield return (row, column);
                yield break;
            }

            long rowMin = row - ring;
            long rowMax = row + ring;
            long columnMin = column - ring;
            long columnMax = column + ring;

            for (long c = columnMin; c <= columnMax; c++)
            {
                yield return (rowMin, c);
                yield return (rowMax, c);
            }

            for (long r = rowMin + 1; r <= rowMax - 1; r++)
            {
                yield return (r, columnMin);
                yield return (r, columnMax);
            }
        }

        private static long CellIndex(double value, double cellSizeDegrees) => (long)Math.Floor(value / cellSizeDegrees);
    }
}
