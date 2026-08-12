namespace OsmToolkit.Finders
{
    /// <summary>
    /// Defines finder methods for finding <see cref="OsmEntity"/> instances by <c>tag</c> data.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="OsmEntity"/> instances to find.</typeparam>
    public interface IOsmValueFinder<T> where T : OsmEntity
    {
        /// <summary>
        /// Finds <typeparamref name="T"/> instances that have a specified id.
        /// </summary>
        /// <param name="data">Data for <see cref="IOsmValueFinder{T}"/> to use and search through.</param>
        /// <param name="id">Unique identifier for <typeparamref name="T"/>, must be of values above 0.</param>
        /// <returns>An <see cref="OsmEntity"/> instance that have the specified id if found, otherwise <c>null</c>.</returns>
        T? FindByOsmId(OsmData data, long id);

        /// <summary>
        /// Finds <typeparamref name="T"/> instances that contain a specified tag by only key.
        /// </summary>
        /// <param name="data">Data for <see cref="IOsmValueFinder{T}"/> to use and search through.</param>
        /// <param name="key">The key value to access the corresponding metadata, cannot be <c>null</c> or an empty string.</param>
        /// <returns>An <see cref="OsmData"/> instance with <typeparamref name="T"/> instances that contain the specified tag.</returns>
        OsmData FindByTag(OsmData data, string key);

        /// <summary>
        /// Finds <typeparamref name="T"/> instances that contain a specified tag by key and value.
        /// </summary>
        /// <param name="data">Data for <see cref="IOsmValueFinder{T}"/> to use and search through.</param>
        /// <param name="key">The key value to access the corresponding metadata, cannot be <c>null</c> or an empty string.</param>
        /// <param name="value">The value describing the corresponding metadata, can be <c>null</c>.</param>
        /// <returns>An <see cref="OsmData"/> instance with <typeparamref name="T"/> instances that contain the specified tag and value.</returns>
        OsmData FindByTag(OsmData data, string key, string? value = null);

        /// <summary>
        /// Finds <typeparamref name="T"/> instances that contain the specified tags in a dictionary with key-value string pairs.
        /// </summary>
        /// <param name="data">Data for <see cref="IOsmValueFinder{T}"/> to use and search through.</param>
        /// <param name="tags">Dictionary containing key-value string pairs describing the metadata of the <typeparamref name="T"/> instances.</param>
        /// <returns>An <see cref="OsmData"/> instance with <typeparamref name="T"/> instances that contain the specified tags and values.</returns>
        OsmData FindByTags(OsmData data, Dictionary<string, string> tags);
    }
}
