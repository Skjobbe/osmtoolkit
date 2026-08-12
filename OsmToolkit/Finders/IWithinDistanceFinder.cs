namespace OsmToolkit.Finders
{
    /// <summary>
    /// Defines finder methods for finding nearby <see cref="OsmEntity"/> instances within a specified <c>radius</c>.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="OsmEntity"/> instances to find.</typeparam>
    public interface IWithinDistanceFinder<T> where T : OsmEntity
    {
        /// <summary>
        /// Finds <typeparamref name="T"/> instances from a specified <see cref="Node"/>'s that are within a specified radius.
        /// </summary>
        /// <param name="data">Data for <see cref="IWithinDistanceFinder{T}"/> to use and search through.</param>
        /// <param name="node">Node for <see cref="IWithinDistanceFinder{T}"/> to search from.</param>
        /// <param name="radiusMeters">Radius in meters for <see cref="IWithinDistanceFinder{T}"/> to search within.</param>
        /// <returns>An <see cref="OsmData"/> instance with <typeparamref name="T"/> instances that are within radius.</returns>
        OsmData FindNearByRadius(OsmData data, Node node, double radiusMeters);

        /// <summary>
        /// Finds <typeparamref name="T"/> instances from a specified coordinate that are within a specified radius.
        /// </summary>
        /// <param name="data">Data for <see cref="IWithinDistanceFinder{T}"/> to use and search through.</param>
        /// <param name="lat">Latitude for <see cref="IWithinDistanceFinder{T}"/> to search from.</param>
        /// <param name="lon">Longitude for <see cref="IWithinDistanceFinder{T}"/> to search from.</param>
        /// <param name="radiusMeters">Radius in meters for <see cref="IWithinDistanceFinder{T}"/> to search within.</param>
        /// <returns>An <see cref="OsmData"/> instance with <typeparamref name="T"/> instances that are within radius.</returns>
        OsmData FindNearByRadius(OsmData data, double lat, double lon, double radiusMeters);

        /// <summary>
        /// Finds <typeparamref name="T"/> instances from a specified <see cref="Node"/>'s coordinate that are within a specified path distance.
        /// </summary>
        /// <param name="data">Data for <see cref="IWithinDistanceFinder{T}"/> to use and search through.</param>
        /// <param name="node">Node for <see cref="IWithinDistanceFinder{T}"/> to search from.</param>
        /// <param name="distanceMeters">Distance in meters for <see cref="IWithinDistanceFinder{T}"/> to search within.</param>
        /// <returns>An <see cref="OsmData"/> instance with <typeparamref name="T"/> instances that are within path distance.</returns>
        OsmData FindNearByPathDistance(OsmData data, Node node, double distanceMeters);

        /// <summary>
        /// Finds <typeparamref name="T"/> instances from a specified coordinate that are within a specified path distance.
        /// </summary>
        /// <param name="data">Data for <see cref="IWithinDistanceFinder{T}"/> to use and search through.</param>
        /// <param name="lat">Latitude for <see cref="IWithinDistanceFinder{T}"/> to search from.</param>
        /// <param name="lon">Longitude for <see cref="IWithinDistanceFinder{T}"/> to search from.</param>
        /// <param name="distanceMeters">Distance in meters for <see cref="IWithinDistanceFinder{T}"/> to search within.</param>
        /// <returns>An <see cref="OsmData"/> instance with <typeparamref name="T"/> instances that are within path distance.</returns>
        OsmData FindNearByPathDistance(OsmData data, double lat, double lon, double distanceMeters);
    }
}
