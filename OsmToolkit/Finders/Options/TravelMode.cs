namespace OsmToolkit.Finders
{
    /// <summary>
    /// Selects forms of transport or travel to know what type of <c>highway:*</c> to filter out using <see cref="PathOptions"/>. Follows <seealso href="https://wiki.openstreetmap.org/wiki/OSM_tags_for_routing/Access_restrictions#Norway"/>.
    /// </summary>
    public enum TravelMode
    {
        /// <summary>
        /// The default travel mode with no specific filters. (does allow pathing in opposite of one-way)
        /// </summary>
        Any,
        /// <summary>
        /// Travel by foot adds filters to not path along disallowed paths. (does allow pathing in opposite of one-way)
        /// </summary>
        Foot,
        /// <summary>
        /// Travel by bicycle adds filters to not path along disallowed paths.
        /// </summary>
        Bicycle,
        /// <summary>
        /// Travel by moped adds filters to not path along disallowed paths.
        /// </summary>
        Moped,
        /// <summary>
        /// Travel by motorcar adds filters to not path along disallowed paths.
        /// </summary>
        MotorCar
    }
}
