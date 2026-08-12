namespace OsmToolkit.Factories
{
    /// <summary>
    /// Factory class responsible for creating instances of <see cref="OsmEntity"/>, such as <see cref="Node"/>, <see cref="Way"/> and <see cref="Relation"/>.
    /// </summary>
    public class OsmEntityFactory : IOsmEntityFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OsmEntityFactory"/> class.
        /// </summary>
        public OsmEntityFactory() { }

        /// <summary>
        /// Initializes a new base <see cref="Node"/> without specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="latitude">Distance north or south of equator, must be within the range of -90 to 90.</param>
        /// <param name="longitude">Distance east or west of prime meridian, must be within the range of -180 to 180.</param>
        /// <returns>A new default instance of <see cref="Node"/> without specified tags.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="latitude"/> is out of the range -90 to 90 or <paramref name="longitude"/> is out of the range -180 to 180.</exception>
        public Node CreateNode(long id, double latitude, double longitude)
            => new Node(id, latitude, longitude);

        /// <summary>
        /// Initializes a new base <see cref="Node"/> with specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="latitude">Distance north or south of equator, must be within the range of -90 to 90.</param>
        /// <param name="longitude">Distance east or west of prime meridian, must be within the range of -180 to 180.</param>
        /// <param name="tags">Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default.</param>
        /// <returns>A new default instance of <see cref="Node"/> with specified tags.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="latitude"/> is out of the range -90 to 90 or <paramref name="longitude"/> is out of the range -180 to 180.</exception>
        public Node CreateNode(long id, double latitude, double longitude, Dictionary<string, string>? tags = null)
            => new Node(id, latitude, longitude, tags);

        /// <summary>
        /// Initializes a new <see cref="Node"/> object without specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="visible">Indicates whether the <see cref="OsmEntity"/> is currently visible or active.
        /// <c>true</c> if it is still part of the active map data, or <c>false</c> if otherwise.</param>
        /// <param name="version">Current version number of <see cref="OsmEntity"/>.</param>
        /// <param name="changeSet">The id of the set of changes the <see cref="OsmEntity"/> was modified in.</param>
        /// <param name="timestamp">Last time <see cref="OsmEntity"/> was changed.</param>
        /// <param name="user">Creator of <see cref="OsmEntity"/>, can be <c>null</c>.</param>
        /// <param name="latitude">Distance north or south of equator, must be within the range of -90 to 90.</param>
        /// <param name="longitude">Distance east or west of prime meridian, must be within the range of -180 to 180.</param>
        /// <returns>A new instance of <see cref="Node"/> without specified tags.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="latitude"/> is out of the range -90 to 90 or <paramref name="longitude"/> is out of the range -180 to 180.</exception>
        public Node CreateNode(long id, bool visible, int version, long changeSet, DateTime timestamp, User user,
            double latitude, double longitude)
            => new Node(id, visible, version, changeSet, timestamp, user, latitude, longitude);

        /// <summary>
        /// Initializes a new <see cref="Node"/> object with specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="visible">Indicates whether the <see cref="OsmEntity"/> is currently visible or active.
        /// <c>true</c> if it is still part of the active map data, or <c>false</c> if otherwise.</param>
        /// <param name="version">Current version number of <see cref="OsmEntity"/>.</param>
        /// <param name="changeSet">The id of the set of changes the <see cref="OsmEntity"/> was modified in.</param>
        /// <param name="timestamp">Last time <see cref="OsmEntity"/> was changed.</param>
        /// <param name="user">Creator of <see cref="OsmEntity"/>, can be <c>null</c>.</param>
        /// <param name="tags"> Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default. </param>
        /// <param name="latitude">Distance north or south of equator, must be within the range of -90 to 90.</param>
        /// <param name="longitude">Distance east or west of prime meridian, must be within the range of -180 to 180.</param>
        /// <returns>A new instance of <see cref="Node"/> with specified tags.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="latitude"/> is out of the range -90 to 90 or <paramref name="longitude"/> is out of the range -180 to 180.</exception>
        public Node CreateNode(long id, bool visible, int version, long changeSet, DateTime timestamp, User user, 
            double latitude, double longitude, Dictionary<string, string>? tags = null)
            => new Node(id, visible, version, changeSet, timestamp, user, latitude, longitude, tags);

        /// <summary>
        /// Initializes a new default <see cref="Way"/> object without specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="nodeReferenceIds">List of reference ids containing the <see cref="Way"/>'s nodes.</param>
        /// <returns>A new default instance of <see cref="Way"/> without specified tags.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="nodeReferenceIds"/> contains less than two elements.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="nodeReferenceIds"/> is <c>null</c>.</exception>
        public Way CreateWay(long id, List<long> nodeReferenceIds)
            => new Way(id, nodeReferenceIds);

        /// <summary>
        /// Initializes a new default <see cref="Way"/> object with specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="nodeReferenceIds">List of reference ids containing the <see cref="Way"/>'s nodes.</param>
        /// <param name="tags">Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default.</param>
        /// <returns>A new default instance of <see cref="Way"/> with specified tags.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="nodeReferenceIds"/> contains less than two elements.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="nodeReferenceIds"/> is <c>null</c>.</exception>
        public Way CreateWay(long id, List<long> nodeReferenceIds, Dictionary<string, string>? tags = null)
            => CreateWay(id, nodeReferenceIds, tags);

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
        /// <returns>A new instance of <see cref="Way"/> without specified tags.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="nodeReferenceIds"/> contains less than two elements.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="nodeReferenceIds"/> is <c>null</c>.</exception>
        public Way CreateWay(long id, bool visible, int version, long changeSet, DateTime timestamp, User user,
            List<long> nodeReferenceIds)
            => new Way(id, visible, version, changeSet, timestamp, user, nodeReferenceIds);

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
        /// <param name="tags"> Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default. </param>
        /// <param name="nodeReferenceIds">List of reference ids containing the <see cref="Way"/>'s nodes.</param>
        /// <returns>A new instance of <see cref="Way"/> with specified tags.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="nodeReferenceIds"/> contains less than two elements.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="nodeReferenceIds"/> is <c>null</c>.</exception>
        public Way CreateWay(long id, bool visible, int version, long changeSet, DateTime timestamp, User user,
            List<long> nodeReferenceIds, Dictionary<string, string>? tags = null)
            => new Way(id, visible, version, changeSet, timestamp, user, nodeReferenceIds, tags);

        /// <summary>
        /// Initializes a new default <see cref="Relation"/> object without specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="members">Collection of the <see cref="Relation"/>'s members.</param>
        /// <returns>A new default instance of <see cref="Relation"/> without specified tags.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="members"/> contain less than one element.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="members"/> is <c>null</c>.</exception>
        public Relation CreateRelation(long id, IList<Member> members)
            => new Relation(id, members);

        /// <summary>
        /// Initializes a new default <see cref="Relation"/> object with specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="members">Collection of the <see cref="Relation"/>'s members.</param>
        /// <param name="tags">Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default.</param>
        /// <returns>A new default instance of<see cref="Relation"/> with specified tags.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="members"/> contain less than one element.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="members"/> is <c>null</c>.</exception>
        public Relation CreateRelation(long id, IList<Member> members, Dictionary<string, string>? tags = null)
            => new Relation(id, members, tags);

        /// <summary>
        /// Initializes a new <see cref="Relation"/> object without specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="visible">Indicates whether the <see cref="OsmEntity"/> is currently visible or active.
        /// <c>true</c> if it is still part of the active map data, or <c>false</c> if otherwise.</param>
        /// <param name="version">Current version number of <see cref="OsmEntity"/>.</param>
        /// <param name="changeSet">The id of the set of changes the <see cref="OsmEntity"/> was modified in.</param>
        /// <param name="timestamp">Last time <see cref="OsmEntity"/> was changed.</param>
        /// <param name="user">Creator of <see cref="OsmEntity"/>, can be <c>null</c>.</param>
        /// <param name="members">Collection of the <see cref="Relation"/>'s members.</param>
        /// <returns>A new instance of <see cref="Relation"/> without specified tags.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="members"/> contain less than one element.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="members"/> is <c>null</c>.</exception>
        public Relation CreateRelation(long id, bool visible, int version, long changeSet, DateTime timestamp, User user,
            List<Member> members)
            => new Relation(id, visible, version, changeSet, timestamp, user, members);

        /// <summary>
        /// Initializes a new <see cref="Relation"/> object with specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="visible">Indicates whether the <see cref="OsmEntity"/> is currently visible or active.
        /// <c>true</c> if it is still part of the active map data, or <c>false</c> if otherwise.</param>
        /// <param name="version">Current version number of <see cref="OsmEntity"/>.</param>
        /// <param name="changeSet">The id of the set of changes the <see cref="OsmEntity"/> was modified in.</param>
        /// <param name="timestamp">Last time <see cref="OsmEntity"/> was changed.</param>
        /// <param name="user">Creator of <see cref="OsmEntity"/>, can be <c>null</c>.</param>
        /// <param name="tags"> Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default. </param>
        /// <param name="members">Collection of the <see cref="Relation"/>'s members.</param>
        /// <returns>A new instance of <see cref="Relation"/> with specified tags.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="members"/> contain less than one element.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="members"/> is <c>null</c>.</exception>
        public Relation CreateRelation(long id, bool visible, int version, long changeSet, DateTime timestamp, User user,
            List<Member> members, Dictionary<string, string>? tags = null)
            => new Relation(id, visible, version, changeSet, timestamp, user, members, tags);

    }
}
