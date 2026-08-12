namespace OsmToolkit.Factories
{
    /// <summary>
    /// Defines factory methods for creating instances of <see cref="OsmEntity"/>, such as <see cref="Node"/>, <see cref="Way"/> and <see cref="Relation"/>.
    /// </summary>
    public interface IOsmEntityFactory
    {
        /// <summary>
        /// Initializes a new base <see cref="Node"/> without specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="latitude">Distance north or south of equator, must be within the range of -90 to 90.</param>
        /// <param name="longitude">Distance east or west of prime meridian, must be within the range of -180 to 180.</param>
        /// <returns>A new default instance of <see cref="Node"/> without specified tags.</returns>
        Node CreateNode(long id, double latitude, double longitude);

        /// <summary>
        /// Initializes a new base <see cref="Node"/> with specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="latitude">Distance north or south of equator, must be within the range of -90 to 90.</param>
        /// <param name="longitude">Distance east or west of prime meridian, must be within the range of -180 to 180.</param>
        /// <param name="tags">Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default.</param>
        ///<returns>A new default instance of<see cref="Node"/> with specified tags.</returns>
        Node CreateNode(long id, double latitude, double longitude, Dictionary<string, string>? tags = null);

        /// <summary>
        /// Initializes a new <see cref="Node"/> object without specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="visible">Indicates whether the <see cref="OsmEntity"/> is currently visible or active.
        /// <c>true</c> if it is still part of the active map data, or <c>false</c> if otherwise.</param>
        /// <param name="version">Current version number of <see cref="OsmEntity"/>.</param>
        /// <param name="changeSet">The id of the set of changes the <see cref="OsmEntity"/> was modified in.</param>
        /// <param name="timestamp">Last time <see cref="OsmEntity"/> was changed.</param>
        /// <param name="user">Creator of <see cref="OsmEntity"/>, can be <c>null</c>..</param>
        /// <param name="latitude">Distance north or south of equator, must be within the range of -90 to 90.</param>
        /// <param name="longitude">Distance east or west of prime meridian, must be within the range of -180 to 180.</param>
        /// <returns>A new instance of <see cref="Node"/> without specified tags.</returns>
        Node CreateNode(long id, bool visible, int version, long changeSet, DateTime timestamp, User user,
            double latitude, double longitude);

        /// <summary>
        /// Initializes a new <see cref="Node"/> object with specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="visible">Indicates whether the <see cref="OsmEntity"/> is currently visible or active.
        /// <c>true</c> if it is still part of the active map data, or <c>false</c> if otherwise.</param>
        /// <param name="version">Current version number of <see cref="OsmEntity"/>.</param>
        /// <param name="changeSet">The id of the set of changes the <see cref="OsmEntity"/> was modified in.</param>
        /// <param name="timestamp">Last time <see cref="OsmEntity"/> was changed.</param>
        /// <param name="user">Creator of <see cref="OsmEntity"/>, can be <c>null</c>..</param>
        /// <param name="tags"> Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default. </param>
        /// <param name="latitude">Distance north or south of equator, must be within the range of -90 to 90.</param>
        /// <param name="longitude">Distance east or west of prime meridian, must be within the range of -180 to 180.</param>
        /// <returns>A new instance of <see cref="Node"/> with specified tags.</returns>
        Node CreateNode(long id, bool visible, int version, long changeSet, DateTime timestamp, User user, 
            double latitude, double longitude, Dictionary<string, string>? tags = null);

        /// <summary>
        /// Initializes a new default <see cref="Way"/> object without specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="nodeReferenceIds">List of reference ids containing the <see cref="Way"/>'s nodes.</param>
        /// <returns>A new default instance of <see cref="Way"/> without specified tags.</returns>
        Way CreateWay(long id, List<long> nodeReferenceIds);

        /// <summary>
        /// Initializes a new default <see cref="Way"/> object with specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="nodeReferenceIds">List of reference ids containing the <see cref="Way"/>'s nodes.</param>
        /// <param name="tags">Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default.</param>
        /// <returns>A new default instance of <see cref="Way"/> with specified tags.</returns>
        Way CreateWay(long id, List<long> nodeReferenceIds, Dictionary<string, string>? tags = null);

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
        Way CreateWay(long id, bool visible, int version, long changeSet, DateTime timestamp, User user,
            List<long> nodeReferenceIds);

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
        Way CreateWay(long id, bool visible, int version, long changeSet, DateTime timestamp, User user, 
            List<long> nodeReferenceIds, Dictionary<string, string>? tags = null);

        /// <summary>
        /// Initializes a new default <see cref="Relation"/> object without specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="members">Collection of the <see cref="Relation"/>'s members.</param>
        /// <returns>A new default instance of <see cref="Relation"/> without specified tags.</returns>
        Relation CreateRelation(long id, IList<Member> members);

        /// <summary>
        /// Initializes a new default <see cref="Relation"/> object with specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="members">Collection of the <see cref="Relation"/>'s members.</param>
        /// <param name="tags">Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default.</param>
        /// <returns>A new default instance of<see cref="Relation"/> with specified tags.</returns>
        Relation CreateRelation(long id, IList<Member> members, Dictionary<string, string>? tags = null);

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
        Relation CreateRelation(long id, bool visible, int version, long changeSet, DateTime timestamp, User user,
            List<Member> members);

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
        Relation CreateRelation(long id, bool visible, int version, long changeSet, DateTime timestamp, User user, 
            List<Member> members, Dictionary<string, string>? tags = null);

    }
}
