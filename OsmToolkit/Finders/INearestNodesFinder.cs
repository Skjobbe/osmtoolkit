namespace OsmToolkit.Finders
{
    /// <summary>
    /// Defines finder methods for finding the nearest <c>possible</c> <see cref="Node"/> instances.
    /// </summary>
    public interface INearestNodesFinder
    {
        /// <summary>
        /// Finds the nearest possible <see cref="Node"/> instance from a specified <see cref="Node"/>'s coordinate.
        /// </summary>
        /// <param name="data">Data for <see cref="INearestNodesFinder"/> to use and search through.</param>
        /// <param name="node">Node for <see cref="INearestNodesFinder"/> to search from.</param>
        /// <returns>Nearest possible <see cref="Node"/> instance, otherwise; <c>null</c>.</returns>
        Node? FindNearestNode(OsmData data, Node node);

        /// <summary>
        /// Finds the nearest possible <see cref="Node"/> instance from a specified <see cref="Node"/>'s coordinate with a tags filter.
        /// </summary>
        /// <param name="data">Data for <see cref="INearestNodesFinder"/> to use and search through.</param>
        /// <param name="node">Node for <see cref="INearestNodesFinder"/> to search from.</param>
        /// <param name="tags">Dictionary containing key-value string pairs describing the metadata of the <see cref="OsmEntity"/> instances.</param>
        /// <returns>Nearest possible <see cref="Node"/> instance, otherwise; <c>null</c>.</returns>
        Node? FindNearestNode(OsmData data, Node node, Dictionary<string, string> tags);

        /// <summary>
        /// Finds the nearest possible <see cref="Node"/> instance from a specified coordinate.
        /// </summary>
        /// <param name="data">Data for <see cref="INearestNodesFinder"/> to use and search through.</param>
        /// <param name="latitude">Latitude for <see cref="INearestNodesFinder"/> to search from.</param>
        /// <param name="longitude">Longitude for <see cref="INearestNodesFinder"/> to search from.</param>
        /// <returns>Nearest possible <see cref="Node"/> instance, otherwise; <c>null</c>.</returns>
        Node? FindNearestNode(OsmData data, double latitude, double longitude);

        /// <summary>
        /// Finds the nearest possible <see cref="Node"/> instance from a specified coordinate with a tags filter.
        /// </summary>
        /// <param name="data">Data for <see cref="INearestNodesFinder"/> to use and search through.</param>
        /// <param name="latitude">Latitude for <see cref="INearestNodesFinder"/> to search from.</param>
        /// <param name="longitude">Longitude for <see cref="INearestNodesFinder"/> to search from.</param>
        /// <param name="tags">Dictionary containing key-value string pairs describing the metadata of the <see cref="OsmEntity"/> instances.</param>
        /// <returns>Nearest possible <see cref="Node"/> instance, otherwise; <c>null</c>.</returns>
        Node? FindNearestNode(OsmData data, double latitude, double longitude, Dictionary<string, string> tags);

        /// <summary>
        /// Finds a limited amount of nearby <see cref="Node"/> instances from a specified <see cref="Node"/>'s coordinate. Allows nodes from the same <see cref="Way"/> and <see cref="Relation"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="INearestNodesFinder"/> to use and search through.</param>
        /// <param name="node">Node for <see cref="INearestNodesFinder"/> to search from.</param>
        /// <param name="limit">Max limit of nearby <see cref="Node"/> instances to find.</param>
        /// <returns>An <see cref="IReadOnlyList{Node}"/> instance with a limited amount of <see cref="Node"/> instances.</returns>
        IReadOnlyList<Node> FindNearbyNodes(OsmData data, Node node, int limit);

        /// <summary>
        /// Finds a limited amount of nearby <see cref="Node"/> instances from a specified <see cref="Node"/>'s coordinate with optional tags and rules for allowing nodes from the <see cref="Way"/> and <see cref="Relation"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="INearestNodesFinder"/> to use and search through.</param>
        /// <param name="node">Node for <see cref="INearestNodesFinder"/> to search from.</param>
        /// <param name="limit">Max limit of nearby <see cref="Node"/> instances to find.</param>
        /// <param name="tags">Dictionary containing key-value string pairs describing the metadata of the <see cref="OsmEntity"/> instances.</param>
        /// <param name="allowSameWay">Whether multiple <see cref="Node"/> instances within the same <see cref="Way"/> can be included in the results.</param>
        /// <param name="allowSameRelation">Whether multiple <see cref="Node"/> instances within the same <see cref="Relation"/> can be included in the results.</param>
        /// <returns>An <see cref="IReadOnlyList{Node}"/> instance with a limited amount of <see cref="Node"/> instances.</returns>
        IReadOnlyList<Node> FindNearbyNodes(OsmData data, Node node, int limit, Dictionary<string, string>? tags = null, bool allowSameWay = true, bool allowSameRelation = true);

        /// <summary>
        /// Finds a limited amount of nearby <see cref="Node"/> instances from a specified coordinate. Allows nodes from the same <see cref="Way"/> and <see cref="Relation"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="INearestNodesFinder"/> to use and search through.</param>
        /// <param name="latitude">Latitude for <see cref="INearestNodesFinder"/> to search from.</param>
        /// <param name="longitude">Longitude for <see cref="INearestNodesFinder"/> to search from.</param>
        /// <param name="limit">>Max limit of nearby <see cref="Node"/> instances to find.</param>
        /// <returns>An <see cref="IReadOnlyList{Node}"/> instance with a limited amount of <see cref="Node"/> instances.</returns>
        IReadOnlyList<Node> FindNearbyNodes(OsmData data, double latitude, double longitude, int limit);

        /// <summary>
        /// Finds a limited amount of nearby <see cref="Node"/> instances from a specified coordinate with optional tags and rules for allowing nodes from the <see cref="Way"/> and <see cref="Relation"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="INearestNodesFinder"/> to use and search through.</param>
        /// <param name="latitude">Latitude for <see cref="INearestNodesFinder"/> to search from.</param>
        /// <param name="longitude">Longitude for <see cref="INearestNodesFinder"/> to search from.</param>
        /// <param name="limit">>Max limit of nearby <see cref="Node"/> instances to find.</param>
        /// <param name="tags">Dictionary containing key-value string pairs describing the metadata of the <see cref="OsmEntity"/> instances.</param>
        /// <param name="allowSameWay">Whether multiple <see cref="Node"/> instances within the same <see cref="Way"/> can be included in the results.</param>
        /// <param name="allowSameRelation">Whether multiple <see cref="Node"/> instances within the same <see cref="Relation"/> can be included in the results.</param>
        /// <returns>An <see cref="IReadOnlyList{Node}"/> instance with a limited amount of <see cref="Node"/> instances.</returns>
        IReadOnlyList<Node> FindNearbyNodes(OsmData data, double latitude, double longitude, int limit, Dictionary<string, string>? tags = null, bool allowSameWay = true, bool allowSameRelation = true);
    }
}
