namespace OsmToolkit.Geocoding
{
    /// <summary>
    /// Thrown when an <see cref="IPlaceLookup"/> finds no place matching the requested name — distinct from an
    /// HTTP-level failure, so callers can tell "no such place" apart from "the geocoding service is unreachable."
    /// </summary>
    public class PlaceNotFoundException : Exception
    {
        /// <summary>
        /// The place name that produced no matches.
        /// </summary>
        public string PlaceName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlaceNotFoundException"/> class.
        /// </summary>
        /// <param name="placeName">The place name that produced no matches.</param>
        public PlaceNotFoundException(string placeName)
            : base($"No place found matching \"{placeName}\".")
        {
            PlaceName = placeName;
        }
    }
}
