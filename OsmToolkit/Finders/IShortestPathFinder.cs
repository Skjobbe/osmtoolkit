namespace OsmToolkit.Finders
{
    /// <summary>
    /// Defines finder methods for finding the <c>shortest</c> possible path.
    /// </summary>
    public interface IShortestPathFinder
    {
        /// <summary>
        /// Finds the shortest possible path made up of <see cref="OsmEntity"/> instances from a specified <c>start</c> <see cref="Node"/>'s coordinate to a specified <c>target</c> <see cref="Node"/>'s coordinate with <c>default</c> <see cref="PathOptions"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="IShortestPathFinder"/> to use and search through.</param>
        /// <param name="startNode">Node for <see cref="IShortestPathFinder"/> to start searching from.</param>
        /// <param name="targetNode">Node for <see cref="IShortestPathFinder"/> to try to reach.</param>
        /// <returns>An <see cref="OsmPath"/> instance with an <see cref="OsmData"/> instance containing the <see cref="OsmEntity"/> instances that make up the shortest valid path, the total distance in meters and <c>startnode</c> and <c>endnode</c> of the path.
        /// If no valid path was found, an empty <see cref="OsmPath"/> instance is returned with a description describing that it could not find a valid path.</returns>
        OsmPath FindShortestPath(OsmData data, Node startNode, Node targetNode);

        /// <summary>
        /// Finds the shortest possible path made up of <see cref="OsmEntity"/> instances from a specified <c>start</c> <see cref="Node"/>'s coordinate to a specified <c>target</c> <see cref="Node"/>'s coordinate with <c>custom</c> <see cref="PathOptions"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="IShortestPathFinder"/> to use and search through.</param>
        /// <param name="startNode">Node for <see cref="IShortestPathFinder"/> to start searching from.</param>
        /// <param name="targetNode">Node for <see cref="IShortestPathFinder"/> to try to reach.</param>
        /// <param name="pathOptions">Options defining which <see cref="Way"/> instances <see cref="IShortestPathFinder"/> can search through.</param>
        /// <returns>An <see cref="OsmPath"/> instance with an <see cref="OsmData"/> instance containing the <see cref="OsmEntity"/> instances that make up the shortest valid path, the total distance in meters and <c>startnode</c> and <c>endnode</c> of the path.
        /// If no valid path was found, an empty <see cref="OsmPath"/> instance is returned with a description describing that it could not find a valid path.</returns>
        OsmPath FindShortestPath(OsmData data, Node startNode, Node targetNode, PathOptions pathOptions);

        /// <summary>
        /// Finds the shortest possible path made up of <see cref="OsmEntity"/> instances from a specified <c>start</c> coordinate to a specified <c>target</c> coordinate with <c>default</c> <see cref="PathOptions"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="IShortestPathFinder"/> to use and search through.</param>
        /// <param name="startLatitude">Latitude for <see cref="IShortestPathFinder"/> to start searching from.</param>
        /// <param name="startLongitude">Longitude for <see cref="IShortestPathFinder"/> to start searching from.</param>
        /// <param name="targetLatitude">Latitude for <see cref="IShortestPathFinder"/> to try to reach.</param>
        /// <param name="targetLongitude">Longitude for <see cref="IShortestPathFinder"/> to try to reach.</param>
        /// <returns>An <see cref="OsmPath"/> instance with an <see cref="OsmData"/> instance containing the <see cref="OsmEntity"/> instances that make up the shortest valid path, the total distance in meters and <c>startnode</c> and <c>endnode</c> of the path.
        /// If no valid path was found, an empty <see cref="OsmPath"/> instance is returned with a description describing that it could not find a valid path.</returns>
        OsmPath FindShortestPath(OsmData data, double startLatitude, double startLongitude, double targetLatitude, double targetLongitude);

        /// <summary>
        /// Finds the shortest possible path made up of <see cref="OsmEntity"/> instances from a specified <c>start</c> coordinate to a specified <c>target</c> coordinate with <c>custom</c> <see cref="PathOptions"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="IShortestPathFinder"/> to use and search through.</param>
        /// <param name="startLatitude">Latitude for <see cref="IShortestPathFinder"/> to start searching from.</param>
        /// <param name="startLongitude">Longitude for <see cref="IShortestPathFinder"/> to start searching from.</param>
        /// <param name="targetLatitude">Latitude for <see cref="IShortestPathFinder"/> to try to reach.</param>
        /// <param name="targetLongitude">Longitude for <see cref="IShortestPathFinder"/> to try to reach.</param>
        /// <param name="pathOptions">Options defining which <see cref="Way"/> instances that <see cref="OsmData"/> can search through.</param>
        /// <returns>An <see cref="OsmPath"/> instance with an <see cref="OsmData"/> instance containing the <see cref="OsmEntity"/> instances that make up the shortest valid path, the total distance in meters and <c>startnode</c> and <c>endnode</c> of the path.
        /// If no valid path was found, an empty <see cref="OsmPath"/> instance is returned with a description describing that it could not find a valid path.</returns>
        OsmPath FindShortestPath(OsmData data, double startLatitude, double startLongitude, double targetLatitude, double targetLongitude, PathOptions pathOptions);
    }
}
