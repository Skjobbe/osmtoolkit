using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OsmToolkit.Extensions;
using OsmToolkit.Finders.Logging;
using OsmToolkit.Finders.Spatial;
using OsmToolkit.Models.Logging;
using System.Runtime.CompilerServices;

namespace OsmToolkit.Finders
{
    /// <summary>
    /// Used for finding specific <see cref="OsmEntity"/> instances in an <see cref="OsmData"/> instance.
    /// </summary>
#pragma warning disable CS0618 // IOsmFinder is obsolete, but still publically needed.
    internal class OsmEntityFinder : IOsmFinder<OsmEntity>, IOsmFinderV2<OsmEntity>, IOsmValueFinder<OsmEntity>, INearestNodesFinder, IWithinDistanceFinder<OsmEntity>, IShortestPathFinder
#pragma warning restore CS0618
    {
        /// <summary>
        /// Grid-based spatial indexes over each <see cref="OsmData"/> instance's nodes, keyed by instance identity
        /// so the index built for a caller's first proximity-search call is reused by later calls against the same
        /// instance, even though <see cref="OsmEntityFinder"/> itself is registered Transient and holds no state
        /// between calls. Mirrors the <see cref="ConditionalWeakTable{TKey,TValue}"/> pattern established in ADR-004
        /// for coalescing in-flight Overpass fetches.
        /// </summary>
        private static readonly ConditionalWeakTable<OsmData, GridNodeIndex> GridIndexByData = new();

        private readonly ILogger<OsmEntityFinder> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="OsmEntityFinder"/> class.
        /// </summary>
        /// <param name="logger">An optional logger diagnostic or debug output. If <c>null</c>, a <see cref="NullLogger{OsmEntityFinder}"/> is used.</param>
        public OsmEntityFinder(ILogger<OsmEntityFinder>? logger = null)
        {
            _logger = logger ?? new NullLogger<OsmEntityFinder>();
        }


        /// <summary>
        /// Finds <see cref="OsmEntity"/> instances that have a specified id.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>, must be of values above 0.</param>
        /// <returns>An <see cref="OsmEntity"/> instance that have the specified id if found, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0.</exception>
        public OsmEntity? FindByOsmId(OsmData data, long id)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(id), $"Id must be greater than 0.");
            }

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(data.Nodes);
            entities.AddRange(data.Ways);
            entities.AddRange(data.Relations);

            List<Node> foundNodes = new List<Node>();
            List<Way> foundWays = new List<Way>();
            List<Relation> foundRelations = new List<Relation>();

            OsmEntity result;

            foreach (OsmEntity entity in entities)
            {
                if (entity.Id == id)
                {
                    if (entity is Node node) foundNodes.Add(node);
                    else if (entity is Way way) foundWays.Add(way);
                    else if (entity is Relation relation) foundRelations.Add(relation);

                    result = entity;
                }
            }

            FinderLogMessages.LogFindByIdResults(_logger, id, foundNodes.Count, foundWays.Count, foundRelations.Count);

            EntityLoggingHelper.LogSummaries(_logger, foundNodes, foundWays, foundRelations);

            if (foundNodes.Count > 0)
                return foundNodes.First();
            else if (foundWays.Count > 0)
                return foundWays.First();
            else if (foundRelations.Count > 0)
                return foundRelations.First();
            else
                return null;
        }

        /// <summary>
        /// Finds <see cref="OsmEntity"/> instances that contain a specified tag by only key.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="key">The key value to access the corresponding metadata, cannot be <c>null</c> or an empty string.</param>
        /// <returns>An <see cref="OsmData"/> instance with <see cref="OsmEntity"/> instances that contain the specified tag.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Throws if <paramref name="key"/> is <c>null</c> or an empty string.</exception>
        public OsmData FindByTag(OsmData data, string key)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"{key} cannot be null or empty string.", nameof(key));
            }

            return FindByTag(data, key, null);
        }

        /// <summary>
        /// Finds <see cref="OsmEntity"/> instances that contain a specified tag by key and value.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="key">The key value to access the corresponding metadata, cannot be <c>null</c> or an empty string.</param>
        /// <param name="value">The value describing the corresponding metadata, can be <c>null</c>.</param>
        /// <returns>An <see cref="OsmData"/> instance with <see cref="OsmEntity"/> instances that contain the specified tag and value.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Throws if <paramref name="key"/> is <c>null</c> or an empty string.</exception>
        public OsmData FindByTag(OsmData data, string key, string? value = null)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException($"{key} cannot be null or empty string.", nameof(key));
            }

            Dictionary<string, string> tags = new Dictionary<string, string>();
            tags.Add(key, value ?? string.Empty);

            OsmData foundData = FindByTags(data, tags);

            FinderLogMessages.LogFindByTagResults(_logger, key, value ?? "", foundData.Nodes.Count, foundData.Ways.Count, foundData.Relations.Count);

            EntityLoggingHelper.LogSummariesTagless(_logger, foundData);

            return foundData;
        }

        /// <summary>
        /// Finds <see cref="OsmEntity"/> instances that contain the specified tags in a dictionary with key-value string pairs.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="tags">Dictionary containing key-value string pairs describing the metadata of the <see cref="OsmEntity"/> instances.</param>
        /// <returns>An <see cref="OsmData"/> instance with <see cref="OsmEntity"/> instances that contain the specified tags and values.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> or <paramref name="tags"/> is <c>null</c>.</exception>
        public OsmData FindByTags(OsmData data, Dictionary<string, string> tags)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (tags == null)
                throw new ArgumentNullException(nameof(tags), "Tags cannot be null, must be defined.");

            List<OsmEntity> entities = new List<OsmEntity>();
            entities.AddRange(data.Nodes);
            entities.AddRange(data.Ways);
            entities.AddRange(data.Relations);

            List<Node> foundNodes = new List<Node>();
            List<Way> foundWays = new List<Way>();
            List<Relation> foundRelations = new List<Relation>();

            foreach (OsmEntity entity in entities)
            {
                bool isNotFound = false;
                foreach (string key in tags.Keys)
                {
                    if (!entity.HasTagKey(key) || (tags[key] != string.Empty && entity.Tags[key] != tags[key]))
                    {
                        isNotFound = true;
                        break;
                    }
                }

                if (!isNotFound)
                {
                    if (entity is Node node) foundNodes.Add(node);
                    else if (entity is Way way) foundWays.Add(way);
                    else if (entity is Relation relation) foundRelations.Add(relation);
                }
            }




            FinderLogMessages.LogFindByTagsResults(_logger, tags.ToTagString(), foundNodes.Count, foundWays.Count, foundRelations.Count);

            EntityLoggingHelper.LogSummaries(_logger, foundNodes, foundWays, foundRelations);

            return new OsmData(data.Header, data.Bounds, foundNodes, foundWays, foundRelations);
        }


        /// <summary>
        /// Finds the nearest possible <see cref="Node"/> instance from a specified <see cref="Node"/>'s coordinate.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="node">Node for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <returns>Nearest possible <see cref="Node"/> instance, otherwise; <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> or <paramref name="node"/> is <c>null</c>.</exception>
        public Node? FindNearestNode(OsmData data, Node node)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (node == null)
                throw new ArgumentNullException(nameof(node), "Node cannot be null, must be defined.");

            return FindNearestNode(data, node.Latitude, node.Longitude);
        }

        /// <summary>
        /// Finds the nearest possible <see cref="Node"/> instance from a specified <see cref="Node"/>'s coordinate with a tags filter.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="node">Node for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="tags">Dictionary containing key-value string pairs describing the metadata of the <see cref="OsmEntity"/> instances.</param>
        /// <returns>Nearest possible <see cref="Node"/> instance, otherwise; <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/>, <paramref name="node"/> or <paramref name="tags"/> is <c>null</c>.</exception>
        public Node? FindNearestNode(OsmData data, Node node, Dictionary<string, string> tags)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (node == null)
                throw new ArgumentNullException(nameof(node), "Node cannot be null, must be defined.");

            if (tags == null)
                throw new ArgumentNullException(nameof(tags), "Tags cannot be null, must be defined.");

            return FindNearestNode(FindByTags(data, tags), node.Latitude, node.Longitude);
        }

        /// <summary>
        /// Finds the nearest possible <see cref="Node"/> instance from a specified coordinate.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="latitude">Latitude for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="longitude">Longitude for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <returns>Nearest possible <see cref="Node"/> instance, otherwise; <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> is <c>null</c>.</exception>
        public Node? FindNearestNode(OsmData data, double latitude, double longitude)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            Node? foundNode = null;

            foreach (Node node in data.Nodes)
            {
                if (foundNode == null || CalculateCoordinatesToMeters(latitude, longitude, node.Latitude, node.Longitude) < CalculateCoordinatesToMeters(latitude, longitude, foundNode.Latitude, foundNode.Longitude))
                    foundNode = node;
            }

            if (foundNode != null)
            {
                var distanceFromStartPoint = CalculateCoordinatesToMeters(latitude, longitude, foundNode.Latitude, foundNode.Longitude);
                FinderLogMessages.LogFindNearestNodeResults(_logger, latitude, longitude, foundNode.Id, foundNode.Latitude, foundNode.Longitude, distanceFromStartPoint);
            }
            else
            {
                FinderLogMessages.LogFindNearestNodeUnable(_logger, latitude, longitude);
            }

                return foundNode;
        }

        /// <summary>
        /// Finds the nearest possible <see cref="Node"/> instance from a specified coordinate with a tags filter.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="latitude">Latitude for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="longitude">Longitude for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="tags">Dictionary containing key-value string pairs describing the metadata of the <see cref="OsmEntity"/> instances.</param>
        /// <returns>Nearest possible <see cref="Node"/> instance, otherwise; <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> or <paramref name="tags"/> is <c>null</c>.</exception>
        public Node? FindNearestNode(OsmData data, double latitude, double longitude, Dictionary<string, string> tags)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (tags == null)
                throw new ArgumentNullException(nameof(tags), "Tags cannot be null, must be defined.");

            return FindNearestNode(FindByTags(data, tags), latitude, longitude);
        }


        /// <summary>
        /// Finds a limited amount of nearby <see cref="OsmEntity"/> instances from a specified <see cref="Node"/>'s coordinate. Allows nodes from the same <see cref="Way"/> and <see cref="Relation"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="node">Node for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="limit">Max limit of nearby <see cref="OsmEntity"/> instances to find.</param>
        /// <returns>An <see cref="IReadOnlyList{Node}"/> instance with a limited amount of <see cref="Node"/> instances.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> or <paramref name="node"/> is <c>null</c>.</exception>
        public IReadOnlyList<Node> FindNearbyNodes(OsmData data, Node node, int limit)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (node == null)
                throw new ArgumentNullException(nameof(node), "Node cannot be null, must be defined.");

            if (limit <= 0)
                throw new ArgumentOutOfRangeException(nameof(limit), "Limit cannot be lower or equal to zero, must be greater.");

            return FindNearbyNodes(data, node.Latitude, node.Longitude, limit, null, true, true);
        }

        /// <summary>
        /// Finds a limited amount of nearby <see cref="OsmEntity"/> instances from a specified <see cref="Node"/>'s coordinate with optional tags and rules for allowing nodes from the <see cref="Way"/> and <see cref="Relation"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="node">Node for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="limit">Max limit of nearby <see cref="OsmEntity"/> instances to find.</param>
        /// <param name="tags">Dictionary containing key-value string pairs describing the metadata of the <see cref="OsmEntity"/> instances.</param>
        /// <param name="allowSameWay">Whether multiple <see cref="OsmEntity"/> instances within the same <see cref="Way"/> can be included in the results.</param>
        /// <param name="allowSameRelation">Whether multiple <see cref="OsmEntity"/> instances within the same <see cref="Relation"/> can be included in the results.</param>
        /// <returns>An <see cref="IReadOnlyList{Node}"/> instance with a limited amount of <see cref="Node"/> instances.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> or <paramref name="node"/> is <c>null</c>.</exception>
        public IReadOnlyList<Node> FindNearbyNodes(OsmData data, Node node, int limit, Dictionary<string, string>? tags = null, bool allowSameWay = true, bool allowSameRelation = true)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (node == null)
                throw new ArgumentNullException(nameof(node), "Node cannot be null, must be defined.");

            if (limit <= 0)
                throw new ArgumentOutOfRangeException(nameof(limit), "Limit cannot be lower or equal to zero, must be greater.");

            return FindNearbyNodes(data, node.Latitude, node.Longitude, limit, tags, allowSameWay, allowSameRelation);
        }

        /// <summary>
        /// Finds a limited amount of nearby <see cref="OsmEntity"/> instances from a specified coordinate. Allows nodes from the same <see cref="Way"/> and <see cref="Relation"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="latitude">Latitude for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="longitude">Longitude for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="limit">Max limit of nearby <see cref="OsmEntity"/> instances to find.</param>
        /// <returns>An <see cref="IReadOnlyList{Node}"/> instance with a limited amount of <see cref="Node"/> instances.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> is <c>null</c>.</exception>
        public IReadOnlyList<Node> FindNearbyNodes(OsmData data, double latitude, double longitude, int limit)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (limit <= 0)
                throw new ArgumentOutOfRangeException(nameof(limit), "Limit cannot be lower or equal to zero, must be greater.");

            return FindNearbyNodes(data, latitude, longitude, limit, null, true, true);
        }

        /// <summary>
        /// Finds a limited amount of nearby <see cref="OsmEntity"/> instances from a specified coordinate with optional tags and rules for allowing nodes from the <see cref="Way"/> and <see cref="Relation"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="latitude">Latitude for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="longitude">Longitude for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="limit">>Max limit of nearby <see cref="OsmEntity"/> instances to find.</param>
        /// <param name="tags">Dictionary containing key-value string pairs describing the metadata of the <see cref="OsmEntity"/> instances.</param>
        /// <param name="allowSameWay">Whether multiple <see cref="OsmEntity"/> instances within the same <see cref="Way"/> can be included in the results.</param>
        /// <param name="allowSameRelation">Whether multiple <see cref="OsmEntity"/> instances within the same <see cref="Relation"/> can be included in the results.</param>
        /// <returns>An <see cref="IReadOnlyList{Node}"/> instance with a limited amount of <see cref="Node"/> instances.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="limit"/> is lower or equal to zero.</exception>
        public IReadOnlyList<Node> FindNearbyNodes(OsmData data, double latitude, double longitude, int limit, Dictionary<string, string>? tags = null, bool allowSameWay = true, bool allowSameRelation = true)
        {
            FinderLogMessages.LogFindNearbyNodesStart(_logger, latitude, longitude, limit, tags?.Count ?? 0, allowSameWay, allowSameRelation);

            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (limit <= 0)
                throw new ArgumentOutOfRangeException(nameof(limit), "Limit cannot be lower or equal to zero.");

            if (tags != null)
            {
                data = FindByTags(data, tags);
                FinderLogMessages.LogTagFilterResults(_logger, tags.ToTagString(), data.Nodes.Count);
            }

            HashSet<Node> resultNodes = new HashSet<Node>(limit);

            List<(Node node, double distance)> distanceSortedNodes = new();

            foreach (Node node in data.Nodes)
            {
                var distance = CalculateCoordinatesToMeters(latitude, longitude, node.Latitude, node.Longitude);
                distanceSortedNodes.Add((node, distance));
                FinderLogMessages.LogDistanceOfNodesToTarget(_logger, node.Id, distance);
            }

            distanceSortedNodes.Sort((a, b) => a.distance.CompareTo(b.distance));

            for (int i = 0; distanceSortedNodes.Count > i; i++)
            {
                if (resultNodes.Count >= limit)
                    break;
                
                var queryNode = distanceSortedNodes[i].node;
                if ((allowSameWay && allowSameRelation) || !IsInSameWayOrRelation(queryNode, resultNodes, data, allowSameWay, allowSameRelation))
                {
                    resultNodes.Add(queryNode);
                }
            }

            string result = string.Join(", ", resultNodes.Select(n => $"{n.Id}: ({n.Latitude}, {n.Longitude})"));
            FinderLogMessages.LogFindNearbyNodesResult(_logger, resultNodes.Count, latitude, longitude, result);
            return resultNodes.ToList();
        }

        /// <summary>
        /// Finds <see cref="OsmEntity"/> instances from a specified coordinate that are within a specified radius.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="node">Node for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="radiusMeters">Radius in meters for <see cref="OsmEntityFinder"/> to search within.</param>
        /// <returns>An <see cref="OsmData"/> instance with <see cref="OsmEntity"/> instances that are within radius.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> or <paramref name="node"/> is <c>null</c>.</exception>
        public OsmData FindNearByRadius(OsmData data, Node node, double radiusMeters)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (node == null)
                throw new ArgumentNullException(nameof(node), "Node cannot be null, must be defined.");

            return FindNearByRadius(data, node.Latitude, node.Longitude, radiusMeters);
        }

        /// <summary>
        /// Finds <see cref="OsmEntity"/> instances from a specified coordinate that are within a specified radius.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="latitude">Latitude for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="longitude">Longitude for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="radiusMeters">Radius in meters for <see cref="OsmEntityFinder"/> to search within.</param>
        /// <returns>An <see cref="OsmData"/> instance with <see cref="OsmEntity"/> instances that are within radius.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> is <c>null</c>.</exception>
        public OsmData FindNearByRadius(OsmData data, double latitude, double longitude, double radiusMeters)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            HashSet<Node> resultNodes = new HashSet<Node>(GetGridIndex(data).FindWithinRadius(latitude, longitude, radiusMeters));

            var (waysInRange, relationsInRange) = GatherNodeRelatedEntities(data, resultNodes);

            var results = new OsmData(data.Header, data.Bounds, resultNodes, waysInRange, relationsInRange);
            FinderLogMessages.LogFindNearByRadiusResults(_logger, latitude, longitude, radiusMeters, results.Nodes.Count, results.Ways.Count, results.Relations.Count);
            EntityLoggingHelper.LogSummariesTagless(_logger, results);

            return results;
        }


        /// <summary>
        /// Finds <see cref="OsmEntity"/> instances from a specified coordinate that are within a specified path distance.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="node">Node for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="distanceMeters">Distance in meters for <see cref="OsmEntityFinder"/> to search within.</param>
        /// <returns>An <see cref="OsmData"/> instance with <see cref="OsmEntity"/> instances that are within path distance.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> or <paramref name="node"/> is <c>null</c>.</exception>
        public OsmData FindNearByPathDistance(OsmData data, Node node, double distanceMeters)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (node == null)
                throw new ArgumentNullException(nameof(node), "Node cannot be null, must be defined.");

            if (data.Nodes.Count == 0)
            {
                FinderLogMessages.LogFindNearByPathDistanceResults(_logger, distanceMeters, node.Latitude, node.Longitude, 0, 0, 0);
                return new OsmData(data.Header, data.Bounds);
            }

            return FindNearByPathDistance(data, node.Latitude, node.Longitude, distanceMeters);
        }

        /// <summary>
        /// Finds <see cref="OsmEntity"/> instances from a specified coordinate that are within a specified path distance.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="latitude">Latitude for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="longitude">Longitude for <see cref="OsmEntityFinder"/> to search from.</param>
        /// <param name="distanceMeters">Distance in meters for <see cref="OsmEntityFinder"/> to search within.</param>
        /// <returns>An <see cref="OsmData"/> instance with <see cref="OsmEntity"/> instances that are within path distance.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> is <c>null</c>.</exception>
        public OsmData FindNearByPathDistance(OsmData data, double latitude, double longitude, double distanceMeters)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (data.Nodes.Count == 0)
            {
                FinderLogMessages.LogFindNearByPathDistanceResults(_logger, distanceMeters, latitude, longitude, 0, 0, 0);
                return new OsmData(data.Header, data.Bounds);
            }

            var graphBuilderResult = BuildGraph(data);
            var nodeDictionary = graphBuilderResult.NodeDictionary;
            var graph = graphBuilderResult.Graph;

            Node startNode = FindNearbyNodes(data, latitude, longitude, 5).First();

            var resultNodes = CalculateNodesByPathDistance(graphBuilderResult, startNode, distanceMeters);

            var (waysInRange, relationsInRange) = GatherNodeRelatedEntities(data, resultNodes);

            var results = new OsmData(data.Header, data.Bounds, resultNodes, waysInRange, relationsInRange);
            FinderLogMessages.LogFindNearByPathDistanceResults(_logger, latitude, longitude, distanceMeters, results.Nodes.Count, results.Ways.Count, results.Relations.Count);
            EntityLoggingHelper.LogSummariesTagless(_logger, results);

            return results;
        }


        /// <summary>
        /// Finds the shortest possible path made up of <see cref="OsmEntity"/> instances from a specified <c>start</c> <see cref="Node"/>'s coordinate to a specified <c>target</c> <see cref="Node"/>'s coordinate with <c>default</c> <see cref="PathOptions"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="startNode">Node for <see cref="OsmEntityFinder"/> to start searching from.</param>
        /// <param name="targetNode">Node for <see cref="OsmEntityFinder"/> to try to reach.</param>
        /// <returns>An <see cref="OsmPath"/> instance with an <see cref="OsmData"/> instance containing the <see cref="OsmEntity"/> instances that make up the shortest valid path, the total distance in meters and <c>startnode</c> and <c>endnode</c> of the path.
        /// If no valid path was found, an empty <see cref="OsmPath"/> instance is returned with a description describing that it could not find a valid path.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/>, <paramref name="startNode"/> or <paramref name="targetNode"/> is <c>null</c>.</exception>
        public OsmPath FindShortestPath(OsmData data, Node startNode, Node targetNode) => FindShortestPath(data, startNode, targetNode, new PathOptions());

        /// <summary>
        /// Finds the shortest possible path made up of <see cref="OsmEntity"/> instances from a specified <c>start</c> <see cref="Node"/>'s coordinate to a specified <c>target</c> <see cref="Node"/>'s coordinate with <c>custom</c> <see cref="PathOptions"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="startNode">Node for <see cref="OsmEntityFinder"/> to start searching from.</param>
        /// <param name="targetNode">Node for <see cref="OsmEntityFinder"/> to try to reach.</param>
        /// <param name="pathOptions">Options defining which <see cref="Way"/> instances <see cref="OsmEntityFinder"/> can search through.</param>
        /// <returns>An <see cref="OsmPath"/> instance with an <see cref="OsmData"/> instance containing the <see cref="OsmEntity"/> instances that make up the shortest valid path, the total distance in meters and <c>startnode</c> and <c>endnode</c> of the path.
        /// If no valid path was found, an empty <see cref="OsmPath"/> instance is returned with a description describing that it could not find a valid path.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/>, <paramref name="startNode"/> or <paramref name="targetNode"/> is <c>null</c>.</exception>
        public OsmPath FindShortestPath(OsmData data, Node startNode, Node targetNode, PathOptions pathOptions)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (startNode == null)
                throw new ArgumentNullException(nameof(startNode), "Node cannot be null, must be defined.");

            if (targetNode == null)
                throw new ArgumentNullException(nameof(targetNode), "Node cannot be null, must be defined.");

            return FindShortestPath(data, startNode.Latitude, startNode.Longitude, targetNode.Latitude, targetNode.Longitude, pathOptions);
        }

        /// <summary>
        /// Finds the shortest possible path made up of <see cref="OsmEntity"/> instances from a specified <c>start</c> coordinate to a specified <c>target</c> coordinate with <c>default</c> <see cref="PathOptions"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="startLatitude">Latitude for <see cref="OsmEntityFinder"/> to start searching from.</param>
        /// <param name="startLongitude">Longitude for <see cref="OsmEntityFinder"/> to start searching from.</param>
        /// <param name="targetLatitude">Latitude for <see cref="OsmEntityFinder"/> to try to reach.</param>
        /// <param name="targetLongitude">Longitude for <see cref="OsmEntityFinder"/> to try to reach.</param>
        /// <returns>An <see cref="OsmPath"/> instance with an <see cref="OsmData"/> instance containing the <see cref="OsmEntity"/> instances that make up the shortest valid path, the total distance in meters and <c>startnode</c> and <c>endnode</c> of the path.
        /// If no valid path was found, an empty <see cref="OsmPath"/> instance is returned with a description describing that it could not find a valid path.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> is <c>null</c>.</exception>
        public OsmPath FindShortestPath(OsmData data, double startLatitude, double startLongitude, double targetLatitude, double targetLongitude) => FindShortestPath(data, startLatitude, startLongitude, targetLatitude, targetLongitude, new PathOptions());

        /// <summary>
        /// Finds the shortest possible path made up of <see cref="OsmEntity"/> instances from a specified <c>start</c> coordinate to a specified <c>target</c> coordinate with <c>custom</c> <see cref="PathOptions"/>.
        /// </summary>
        /// <param name="data">Data for <see cref="OsmEntityFinder"/> to use and search through.</param>
        /// <param name="startLatitude">Latitude for <see cref="OsmEntityFinder"/> to start searching from.</param>
        /// <param name="startLongitude">Longitude for <see cref="OsmEntityFinder"/> to start searching from.</param>
        /// <param name="targetLatitude">Latitude for <see cref="OsmEntityFinder"/> to try to reach.</param>
        /// <param name="targetLongitude">Longitude for <see cref="OsmEntityFinder"/> to try to reach.</param>
        /// <param name="pathOptions">Options defining which <see cref="Way"/> instances <see cref="OsmEntityFinder"/> can search through.</param>
        /// <returns>An <see cref="OsmPath"/> instance with an <see cref="OsmData"/> instance containing the <see cref="OsmEntity"/> instances that make up the shortest valid path, the total distance in meters and <c>startnode</c> and <c>endnode</c> of the path.
        /// If no valid path was found, an empty <see cref="OsmPath"/> instance is returned with a description describing that it could not find a valid path.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="data"/> is <c>null</c>.</exception>
        public OsmPath FindShortestPath(OsmData data, double startLatitude, double startLongitude, double targetLatitude, double targetLongitude, PathOptions pathOptions)
        {
            if (pathOptions == null)
            {
                pathOptions = new PathOptions();
            }

            if (data == null)
                throw new ArgumentNullException(nameof(data), "Data cannot be null, must be defined.");

            if (data.Nodes.Count == 0)
            {
                FinderLogMessages.LogFindShortestPathResult(_logger, startLatitude, startLongitude, targetLatitude, targetLongitude, pathOptions.AvoidMotorway, pathOptions.Mode, 0, 0, 0, 0, 0, 0);
                return new OsmPath(new OsmData(data.Header, data.Bounds), 0, null, null, "No valid path could be found.");
            }

            

            GraphBuilderResult graphBuilderResult = BuildGraph(data, pathOptions);

            Node? startNode = FindNodeInValidPathWay(data, pathOptions, (List<Node>)FindNearbyNodes(data, startLatitude, startLongitude, 5, null, false, false));
            Node? targetNode = FindNodeInValidPathWay(data, pathOptions, (List<Node>)FindNearbyNodes(data, targetLatitude, targetLongitude, 5, null, false, false));

            if (startNode == null || targetNode == null)
                return new OsmPath(new OsmData(data.Header, data.Bounds), 0, null, null, "No valid path could be found.");

            var (totalDistance, pathNodeIds) = CalculateShortestPath(graphBuilderResult, startNode, targetNode);

            if (pathNodeIds.Count < 2 || pathNodeIds[0] != startNode.Id)
                return new OsmPath(new OsmData(data.Header, data.Bounds), 0, null, null, "No valid path could be found.");

            var (pathNodes, pathWays, pathRelations) = GatherPathRelatedEntities(data, pathNodeIds, graphBuilderResult);


            FinderLogMessages.LogFindShortestPathResult(_logger, startLatitude, startLongitude, targetLatitude, targetLongitude, pathOptions.AvoidMotorway, pathOptions.Mode, pathNodes.Count, pathWays.Count, pathRelations.Count, totalDistance, startNode.Id, targetNode.Id);
            EntityLoggingHelper.LogSummariesTagless(_logger, pathNodes, pathWays, pathRelations);

            return new OsmPath(new OsmData(data.Header, data.Bounds, pathNodes, pathWays, pathRelations), totalDistance, startNode, targetNode);
        }

        private static double CalculateCoordinatesToMeters(double latitudeFrom, double longitudeFrom, double latitudeTo, double longitudeTo)
            => GeoDistance.HaversineMeters(latitudeFrom, longitudeFrom, latitudeTo, longitudeTo);

        /// <summary>
        /// Returns the grid-based spatial index over <paramref name="data"/>'s nodes, building it lazily on first
        /// use and reusing it for every later call against the same <see cref="OsmData"/> instance.
        /// </summary>
        private static GridNodeIndex GetGridIndex(OsmData data)
            => GridIndexByData.GetValue(data, static d => GridNodeIndex.Build(d.Nodes));

        private bool IsInSameWayOrRelation(Node queryNode, IEnumerable<Node> resultNodes, OsmData data, bool allowSameWay, bool allowSameRelation)
        {
            foreach (Node resultNode in resultNodes)
            {
                if ((!allowSameWay && AreInSameWay(resultNode, queryNode, data)) || (!allowSameRelation && AreInSameRelation(resultNode, queryNode, data)))
                {
                    return true;
                }
            }
            return false;
        }
        private bool AreInSameRelation(Node a, Node b, OsmData data)
        {
            foreach (var relation in data.Relations)
            {
                if (relation.Members.Any(m => m.ReferenceId == a.Id) &&
                    relation.Members.Any(m => m.ReferenceId == b.Id))
                    return true;
            }
            return false;
        }

        private bool AreInSameWay(Node a, Node b, OsmData data)
        {
            foreach (var way in data.Ways)
            {
                if (way.NodeReferenceIds.Contains(a.Id) && way.NodeReferenceIds.Contains(b.Id))
                    return true;
            }
            return false;
        }

        private (IEnumerable<Way>, IEnumerable<Relation>) GatherNodeRelatedEntities(OsmData data, HashSet<Node> resultNodes)
        {
            var nodeIdsInRange = new HashSet<long>(resultNodes.Select(node => node.Id));

            var waysInRange = data.Ways.Where(
                way => way.NodeReferenceIds.Any(id => nodeIdsInRange.Contains(id)));
            var wayIdsInRange = new HashSet<long>(waysInRange.Select(way => way.Id));

            var relationsInRange = data.Relations.Where(relation =>
                relation.Members.Any(member =>
                    nodeIdsInRange.Contains(member.ReferenceId) ||
                    wayIdsInRange.Contains(member.ReferenceId)
                ));

            return (waysInRange, relationsInRange);
        }

        private (List<Node>, List<Way>, List<Relation>) GatherPathRelatedEntities(OsmData data, List<long> pathNodeIds, GraphBuilderResult graphBuilderResult)
        {
            var nodeDictionary = graphBuilderResult.NodeDictionary;
            var waySegments = graphBuilderResult.WaySegments;

            var pathNodes = pathNodeIds.Select(id => nodeDictionary[id]).ToList();

            var pathWays = new HashSet<Way>();
            for (int i = 0; i < pathNodeIds.Count - 1; i++)
            {
                var key = (pathNodeIds[i], pathNodeIds[i + 1]);
                if (waySegments.TryGetValue(key, out var way))
                    pathWays.Add(way);
            }

            var pathWayIds = new HashSet<long>(pathWays.Select(w => w.Id));
            var pathNodeIdSet = new HashSet<long>(pathNodeIds);

            var pathRelations = data.Relations.Where(relation =>
                relation.Members.Any(member =>
                    pathNodeIdSet.Contains(member.ReferenceId) ||
                    pathWayIds.Contains(member.ReferenceId)
                ));

            return (pathNodes, pathWays.ToList(), pathRelations.ToList());
        }

        private HashSet<Node> CalculateNodesByPathDistance(GraphBuilderResult graphBuilderResult, Node startNode, double distanceMeters)
        {
            var nodeDictionary = graphBuilderResult.NodeDictionary;
            var graph = graphBuilderResult.Graph;

            // Dijkstra
            var visited = new HashSet<long>();
            var queue = new Queue<(long nodeId, double pathDistance)>();
            var resultNodes = new HashSet<Node>();

            queue.Enqueue((startNode.Id, 0));
            visited.Add(startNode.Id);

            while (queue.Count > 0)
            {
                var (currentId, currentDistance) = queue.Dequeue();
                resultNodes.Add(nodeDictionary[currentId]);

                if (!graph.ContainsKey(currentId))
                    continue;

                foreach (var (neighborId, edgeDistance) in graph[currentId])
                {
                    double newDistance = currentDistance + edgeDistance;
                    if (newDistance <= distanceMeters && !visited.Contains(neighborId))
                    {
                        visited.Add(neighborId);
                        queue.Enqueue((neighborId, newDistance));
                    }
                }
            }

            return resultNodes;
        }

        private (double, List<long>) CalculateShortestPath(GraphBuilderResult graphBuilderResult, Node startNode, Node targetNode)
        {
            var nodeDictionary = graphBuilderResult.NodeDictionary;
            var graph = graphBuilderResult.Graph;

            // Dijkstra
            var previous = new Dictionary<long, long?>();
            var distances = new Dictionary<long, double>();
            var queue = new PriorityQueue<long, double>();

            foreach (var node in nodeDictionary.Keys)
            {
                distances[node] = double.PositiveInfinity;
                previous[node] = null;
            }

            distances[startNode.Id] = 0;
            queue.Enqueue(startNode.Id, 0);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();

                if (currentId == targetNode.Id)
                    break;

                if (!graph.ContainsKey(currentId))
                    continue;

                foreach (var (neighborId, edgeDistance) in graph[currentId])
                {
                    double alternativeDistance = distances[currentId] + edgeDistance;
                    if (alternativeDistance <= distances[neighborId])
                    {
                        distances[neighborId] = alternativeDistance;
                        previous[neighborId] = currentId;
                        queue.Enqueue(neighborId, alternativeDistance);
                    }
                }
            }

            var pathNodeIds = new List<long>();
            long? step = targetNode.Id;
            while (step != null)
            {
                pathNodeIds.Insert(0, step.Value);
                step = previous[step.Value];
            }

            return (distances[targetNode.Id], pathNodeIds);
        }

        private Node? FindNodeInValidPathWay(OsmData data, PathOptions pathOptions, List<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                foreach (Way way in data.Ways)
                {
                    if (!node.IsPartOf(way) || !IsValidPathWay(way, pathOptions))
                        continue;

                    return node;
                }
            }

            return null;
        }

        private void AddEdge(Dictionary<long, List<(long, double)>> graph, long fromId, long toId, double distance)
        {
            if (!graph.TryGetValue(fromId, out var neighbors))
            {
                neighbors = new List<(long, double)>();
                graph[fromId] = neighbors;
            }

            neighbors.Add((toId, distance));
        }
        private GraphBuilderResult BuildGraph(OsmData data)
        {
            return BuildGraph(data, null);
        }

        private GraphBuilderResult BuildGraph(OsmData data, PathOptions? pathOptions = null)
        {

            var nodeDictionary = data.Nodes.ToDictionary(node => node.Id);
            var waySegments = new Dictionary<(long, long), Way>();

            var graph = new Dictionary<long, List<(long neighborId, double edgeLength)>>();
            foreach (var way in data.Ways)
            {
                if (!IsValidPathWay(way, pathOptions))
                    continue;

                for (int i = 0; i < way.NodeReferenceIds.Count - 1; i++)
                {
                    var fromNodeId = way.NodeReferenceIds[i];
                    var toNodeId = way.NodeReferenceIds[i + 1];
                    if (nodeDictionary.TryGetValue(fromNodeId, out Node? fromNode) && nodeDictionary.TryGetValue(toNodeId, out Node? toNode))
                    {
                        double distance = CalculateCoordinatesToMeters(fromNode.Latitude, fromNode.Longitude, toNode.Latitude, toNode.Longitude);

                        AddEdge(graph, fromNodeId, toNodeId, distance);
                        waySegments[(fromNodeId, toNodeId)] = way;

                        if (!way.IsOneWay() || (pathOptions != null && (pathOptions.Mode == TravelMode.Foot || pathOptions.Mode == TravelMode.Any)))
                        {
                            AddEdge(graph, toNodeId, fromNodeId, distance);
                            waySegments[(toNodeId, fromNodeId)] = way;
                        }
                    }
                }
            }

            return new GraphBuilderResult(graph, nodeDictionary, waySegments);
        }

        private bool IsValidPathWay(Way way, PathOptions? pathOptions)
        {
            if (way.HasTagValue("roundabout") && !way.HasAnyTag(("oneyway", "yes")))
                way.AddTag("oneway", "yes");

            if (pathOptions == null)
                return true; 

            if (!way.HasAnyTagKey(["highway"]) 
                || (pathOptions.AvoidMotorway && way.HasAnyTagValue(["motorway", "trunk", "motorway_link", "trunk_link"]))
                || way.HasAnyTag(("access", "no"), ("highway", "escape"), ("access", "private")))
                return false;

            switch (pathOptions.Mode)
            {
                case TravelMode.Any:
                    break;

                case TravelMode.Foot:
                    if (way.HasAnyTagValue(["motorway", "bridleway", "motorway_link", "busway"]))
                        return false;

                    if (way.HasAnyTag(("foot", "no"), ("motorroad", "yes"), ("foot", "private")))
                        return false;
                    break;

                case TravelMode.Bicycle:
                    if (way.HasAnyTagValue(["motorway", "bridleway", "motorway_link", "busway"]))
                        return false;

                    if (way.HasAnyTag(("bicycle", "no"), ("motorroad", "yes"), ("bicycle", "private")))
                        return false;
                    break;

                case TravelMode.Moped:
                    if (way.HasAnyTagValue(["motorway", "bridleway", "motorway_link", "path", "cycleway", "footway", "pedestrian", "steps", "corridor", "busway"]))
                        return false;

                    if (way.HasAnyTag(("motorroad", "yes"), ( "vehicle", "no"), ("motor_vehicle", "no")))
                        return false;
                    break;

                case TravelMode.MotorCar:
                    if (way.HasAnyTagValue(["bridleway", "path", "cycleway", "footway", "pedestrian", "steps", "corridor", "busway"]))
                        return false;
                    
                    if (way.HasAnyTag(("vehicle", "no"), ("motor_vehicle", "no"), ("vehicle", "private")))
                        return false;
                    break;

                default:
                    _logger.LogWarning("No expected TravelMode selected.");
                    break;
            }

            return true;
        } 
        
    }
}
