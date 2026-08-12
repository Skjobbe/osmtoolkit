namespace OsmToolkit.Finders
{
    /// <summary>
    /// Represents options for path traversal through <see cref="Way"/> instances.
    /// </summary>
    public class PathOptions
    {
        /// <summary>
        /// <see cref="TravelMode"/> to determine which paths can be traversed, with <c>Any</c> as default on initialize.
        /// </summary>
        public TravelMode Mode { get; init; } = TravelMode.Any;

        /// <summary>
        /// Whether to avoid motorways or not, with <c>false</c> as default on initialize.
        /// </summary>
        public bool AvoidMotorway { get; init; } = false;

        /// <summary>
        /// Initializes a new <see cref="PathOptions"/> object with <c>default</c> options.
        /// </summary>
        public PathOptions() { }

        /// <summary>
        /// Initializes a new <see cref="PathOptions"/> object.
        /// </summary>
        /// <param name="mode">Form of transport or travel to determine the paths that can be traversed.</param>
        /// <param name="avoidMotorway">Whether to avoid motorways or not.</param>
        public PathOptions(TravelMode mode, bool avoidMotorway)
        {
            Mode = mode;
            AvoidMotorway = avoidMotorway;
        }
    }
}
