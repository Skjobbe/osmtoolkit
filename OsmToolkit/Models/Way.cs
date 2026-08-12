using System.Text.Json.Serialization;
namespace OsmToolkit
{
    /// <summary>
    /// Connection between two or more nodes.
    /// </summary>
    public class Way : OsmEntity
    {
        private List<long> _nodeReferenceIds;
        /// <summary>
        /// A read-only list of reference ids containing the <see cref="Way"/>'s nodes.
        /// </summary>
        [JsonPropertyName("nodeRefs"), JsonPropertyOrder(6)]
        public IReadOnlyList<long> NodeReferenceIds => _nodeReferenceIds;

        /// <summary>
        /// Initializes a new default <see cref="Way"/> object without specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="nodeReferenceIds">List of reference ids containing the <see cref="Way"/>'s nodes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="nodeReferenceIds"/> contains less than two elements.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="nodeReferenceIds"/> is <c>null</c>.</exception>
        public Way(long id, List<long> nodeReferenceIds) : this(id, nodeReferenceIds, null) { }

        /// <summary>
        /// Initializes a new default <see cref="Way"/> object with specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="nodeReferenceIds">List of reference ids containing the <see cref="Way"/>'s nodes.</param>
        /// <param name="tags">Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="nodeReferenceIds"/> contains less than two elements.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="nodeReferenceIds"/> is <c>null</c>.</exception>
        public Way(long id, List<long> nodeReferenceIds, Dictionary<string, string>? tags = null)
            : base(id, tags)
        {
            if (nodeReferenceIds is null)
            {
                throw new ArgumentNullException(nameof(nodeReferenceIds), $"NodeIdReferences cannot be null.");
            }
            if (nodeReferenceIds.Count < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(nodeReferenceIds), $"NodeIdReferences must have at least two elements.");
            }

            _nodeReferenceIds = nodeReferenceIds;
        }

        /// <summary>
        /// Initializes a new <see cref="Way"/> object without specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="visible">Indicates whether the <see cref="OsmEntity"/> is currently visible or active.
        /// <c>true</c> if it is still part of the active map data, or <c>false</c> if otherwise.</param>
        /// <param name="version">Current version number of <see cref="OsmEntity"/>.</param>
        /// <param name="changeSet">The id of the set of changes the <see cref="OsmEntity"/> was modified in.</param>
        /// <param name="timestamp">Last time <see cref="OsmEntity"/> was changed.</param>
        /// <param name="user">Creator of <see cref="OsmEntity"/>, can be <c>null</c>.</param>
        /// <param name="nodeReferenceIds">List of reference ids containing the <see cref="Way"/>'s nodes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="nodeReferenceIds"/> contains less than two elements.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="nodeReferenceIds"/> is <c>null</c>.</exception>
        public Way(long id, bool visible, int version, long changeSet, DateTime timestamp, User user, List<long> nodeReferenceIds)
            : this(id, visible, version, changeSet, timestamp, user, nodeReferenceIds, tags: null) { }

        /// <summary>
        /// Initializes a new <see cref="Way"/> object with specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="visible">Indicates whether the <see cref="OsmEntity"/> is currently visible or active.
        /// <c>true</c> if it is still part of the active map data, or <c>false</c> if otherwise.</param>
        /// <param name="version">Current version number of <see cref="OsmEntity"/>.</param>
        /// <param name="changeSet">The id of the set of changes the <see cref="OsmEntity"/> was modified in.</param>
        /// <param name="timestamp">Last time <see cref="OsmEntity"/> was changed.</param>
        /// <param name="user">Creator of <see cref="OsmEntity"/>, can be <c>null</c>.</param>
        /// <param name="tags">Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default.</param>
        /// <param name="nodeReferenceIds">List of reference ids containing the <see cref="Way"/>'s nodes.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="nodeReferenceIds"/> contains less than two elements.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="nodeReferenceIds"/> is <c>null</c>.</exception>
        public Way(long id, bool visible, int version, long changeSet, DateTime timestamp, User user, List<long> nodeReferenceIds, Dictionary<string, string>? tags = null)
            : base(id, visible, version, changeSet, timestamp, user, tags)
        {
            if (nodeReferenceIds is null)
            {
                throw new ArgumentNullException(nameof(nodeReferenceIds), $"NodeIdReferences cannot be null.");
            }
            if (nodeReferenceIds.Count < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(nodeReferenceIds), $"NodeIdReferences must have at least two elements.");
            }

            _nodeReferenceIds = nodeReferenceIds;
        }

        /// <summary>
        /// Checks if the <see cref="Way"/> is a oneway or not.
        /// </summary>
        /// <returns><c>true</c>, if it is a oneway. Otherwise; <c>false</c>.</returns>
        public bool IsOneWay()
        {
            return this.HasAnyTag(new Dictionary<string, string> { { "oneway", "yes" } });
        }
    }
}
