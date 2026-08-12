namespace OsmToolkit.Finders
{
    /// <summary>
    /// Defines finder methods for finding specific <typeparamref name="T"/> instances in an <see cref="OsmData"/> instance.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="OsmEntity"/> instances to find.</typeparam>
    [Obsolete("Use IOsmFinderV2 instead.", false)]
    public interface IOsmFinder<T> where T : OsmEntity
    {
        /// <summary>
        /// Finds <typeparamref name="T"/> instances that contain a specified tag by only key.
        /// </summary>
        /// <param name="data">Data for <see cref="IOsmFinder{T}"/> to use and search through.</param>
        /// <param name="key">The key value to access the corresponding metadata, cannot be <c>null</c> or an empty string.</param>
        /// <returns>An <see cref="OsmData"/> instance with <typeparamref name="T"/> instances that contain the specified tag.</returns>
        OsmData FindByTag(OsmData data, string key);

        /// <summary>
        /// Finds <typeparamref name="T"/> instances that contain a specified tag by key and value.
        /// </summary>
        /// <param name="data">Data for <see cref="IOsmFinder{T}"/> to use and search through.</param>
        /// <param name="key">The key value to access the corresponding metadata, cannot be <c>null</c> or an empty string.</param>
        /// <param name="value">The value describing the corresponding metadata, can be <c>null</c>.</param>
        /// <returns>An <see cref="OsmData"/> instance with <typeparamref name="T"/> instances that contain the specified tag and value.</returns>
        OsmData FindByTag(OsmData data, string key, string? value = null);

        /// <summary>
        /// Finds <typeparamref name="T"/> instances from a specified coordinate that are within a specified radius.
        /// </summary>
        /// <param name="data">Data for <see cref="IOsmFinder{T}"/> to use and search through.</param>
        /// <param name="lat">Latitude for <see cref="IOsmFinder{T}"/> to search from.</param>
        /// <param name="lon">Longitude for <see cref="IOsmFinder{T}"/> to search from.</param>
        /// <param name="radiusMeters">Radius in meters for <see cref="IOsmFinder{T}"/> to search within.</param>
        /// <returns>An <see cref="OsmData"/> instance with <typeparamref name="T"/> instances that are within radius.</returns>
        OsmData FindNearByRadius(OsmData data, double lat, double lon, double radiusMeters);

        /// <summary>
        /// Finds <typeparamref name="T"/> instances from a specified coordinate that are within a specified path distance.
        /// </summary>
        /// <param name="data">Data for <see cref="IOsmFinder{T}"/> to use and search through.</param>
        /// <param name="lat">Latitude for <see cref="IOsmFinder{T}"/> to search from.</param>
        /// <param name="lon">Longitude for <see cref="IOsmFinder{T}"/> to search from.</param>
        /// <param name="distanceMeters">Distance in meters for <see cref="IOsmFinder{T}"/> to search within.</param>
        /// <returns>An <see cref="OsmData"/> instance with <typeparamref name="T"/> instances that are within path distance.</returns>
        OsmData FindNearByPathDistance(OsmData data, double lat, double lon, double distanceMeters);
    }
}
