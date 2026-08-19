namespace OsmToolkit.Finders.Spatial
{
    /// <summary>
    /// Coordinate-distance helpers shared between <see cref="OsmEntityFinder"/> and <see cref="GridNodeIndex"/>.
    /// </summary>
    internal static class GeoDistance
    {
        private const double EarthRadiusMeters = 6371000d;

        /// <summary>
        /// The approximate distance in meters covered by one degree of latitude, treated as constant across all latitudes.
        /// </summary>
        internal const double MetersPerDegreeLatitude = 111_320d;

        /// <summary>
        /// Latitude is clamped to this magnitude before computing <see cref="MetersPerDegreeLongitude"/>, so that
        /// values near the poles don't collapse the conversion towards zero.
        /// </summary>
        private const double MaxAbsLatitudeDegrees = 89.9d;

        /// <summary>
        /// Computes the great-circle distance between two coordinates, in meters, using the Haversine formula.
        /// </summary>
        internal static double HaversineMeters(double latitudeFrom, double longitudeFrom, double latitudeTo, double longitudeTo)
        {
            double latitudeRadiansFrom = latitudeFrom * Math.PI / 180;
            double latitudeRadiansTo = latitudeTo * Math.PI / 180;
            double latitudeDifference = (latitudeTo - latitudeFrom) * Math.PI / 180;
            double longitudeDifference = (longitudeTo - longitudeFrom) * Math.PI / 180;

            double a = Math.Sin(latitudeDifference / 2) * Math.Sin(latitudeDifference / 2) +
                       Math.Cos(latitudeRadiansFrom) * Math.Cos(latitudeRadiansTo) *
                       Math.Sin(longitudeDifference / 2) * Math.Sin(longitudeDifference / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadiusMeters * c;
        }

        /// <summary>
        /// Computes the approximate distance in meters covered by one degree of longitude at a given latitude,
        /// correcting for longitude's latitude-dependent shrinkage (a degree of longitude covers less real-world
        /// distance further from the equator).
        /// </summary>
        internal static double MetersPerDegreeLongitude(double latitudeDegrees)
        {
            double clampedLatitude = Math.Clamp(latitudeDegrees, -MaxAbsLatitudeDegrees, MaxAbsLatitudeDegrees);
            double cosLatitude = Math.Cos(clampedLatitude * Math.PI / 180d);
            return MetersPerDegreeLatitude * cosLatitude;
        }
    }
}
