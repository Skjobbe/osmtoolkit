using System.Text.Json.Serialization;

namespace OsmToolkit
{
    /// <summary>
    /// Represents the latitude and longitude bounds for OSM-data.
    /// </summary>
    public class OsmCoordinateBounds
    {
        /// <summary>
        /// Minimum distance north or south of equator, within the range of -90 to 90.
        /// </summary>
        [JsonPropertyName("minlat"), JsonPropertyOrder(0)]
        public double MinimumLatitude { get; }
        /// <summary>
        /// Minimum distance east or west of prime meridian, within the range of -180 to 180.
        /// </summary>
        [JsonPropertyName("minlon"), JsonPropertyOrder(1)]
        public double MinimumLongitude { get; }
        /// <summary>
        /// Maximum distance north or south of equator, within the range of -90 to 90.
        /// </summary>
        [JsonPropertyName("maxlat"), JsonPropertyOrder(2)]
        public double MaximumLatitude { get; }
        /// <summary>
        /// Maximum distance east or west of prime meridian, within the range of -180 to 180.
        /// </summary>
        [JsonPropertyName("maxlon"), JsonPropertyOrder(3)]
        public double MaximumLongitude { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OsmCoordinateBounds"/> class.
        /// </summary>
        /// <param name="minimumLatitude">Minimum distance north or south of equator, within the range of -90 to 90.</param>
        /// <param name="minimumLongitude">Minimum distance east or west of prime meridian, within the range of -180 to 180.</param>
        /// <param name="maximumLatitude">Maximum distance north or south of equator, within the range of -90 to 90.</param>
        /// <param name="maximumLongitude">Maximum distance east or west of prime meridian, within the range of -180 to 180.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="minimumLatitude"/> or <paramref name="minimumLatitude"/> is out of the range -90 to 90, <paramref name="minimumLongitude"/> or <paramref name="maximumLongitude"/> is out of the range -180 to 180,
        /// <paramref name="minimumLatitude"/> is equal to or greater than <paramref name="maximumLatitude"/> or <paramref name="minimumLongitude"/> is equal to or greater than <paramref name="maximumLongitude"/>.</exception>
        public OsmCoordinateBounds(double minimumLatitude, double minimumLongitude, double maximumLatitude, double maximumLongitude) 
        {
            if (minimumLatitude < -90 || minimumLatitude > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumLatitude), $"MinimumLatitude must be within the range -90 to 90.");
            }
            if (minimumLongitude < -180 || minimumLongitude > 180)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumLongitude), $"MinimumLongitude must be within the range -180 to 180.");
            }
            if (maximumLatitude < -90 || maximumLatitude > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumLatitude), $"MaximumLatitude must be within the range -90 to 90.");
            }
            if (maximumLongitude < -180 || maximumLongitude > 180)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumLongitude), $"MaximumLongitude must be within the range -180 to 180.");
            }
            if (minimumLatitude >= maximumLatitude)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumLatitude), $"MinimumLatitude cannot be greater than MaximumLatitude: {maximumLatitude}.");
            }
            if (minimumLongitude >= maximumLongitude)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumLongitude), $"MinimumLongitude cannot be greater than MaximumLongitude: {maximumLongitude}.");
            }

            MinimumLatitude = minimumLatitude;
            MinimumLongitude = minimumLongitude;
            MaximumLatitude = maximumLatitude;
            MaximumLongitude = maximumLongitude;
        }
    }
}
