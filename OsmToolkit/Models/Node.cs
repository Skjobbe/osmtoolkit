using System.Text.Json.Serialization;
namespace OsmToolkit
{
    /// <summary>
    /// Represents a point of interest on earth.
    /// </summary>
    public class Node : OsmEntity
    {
        /// <summary>
        /// Distance north or south of equator, within the range of -90 to 90.
        /// </summary>
        [JsonPropertyName("lat"), JsonPropertyOrder(6)]
        public double Latitude { get; }
        /// <summary>
        /// Distance east or west of prime meridian, within the range of -180 to 180.
        /// </summary>
        [JsonPropertyName("lon"), JsonPropertyOrder(7)]
        public double Longitude { get; }

        /// <summary>
        /// Initializes a new base <see cref="Node"/> without specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="latitude">Distance north or south of equator, must be within the range of -90 to 90.</param>
        /// <param name="longitude">Distance east or west of prime meridian, must be within the range of -180 to 180.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="latitude"/> is out of the range -90 to 90 or <paramref name="longitude"/> is out of the range -180 to 180.</exception>
        public Node(long id, double latitude, double longitude)
            : this(id, latitude, longitude, null) { }

        /// <summary>
        /// Initializes a new base <see cref="Node"/> with specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="latitude">Distance north or south of equator, must be within the range of -90 to 90.</param>
        /// <param name="longitude">Distance east or west of prime meridian, must be within the range of -180 to 180.</param>
        /// <param name="tags">Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="latitude"/> is out of the range -90 to 90 or <paramref name="longitude"/> is out of the range -180 to 180.</exception>
        public Node(long id, double latitude, double longitude, Dictionary<string, string>? tags = null)
            : base(id, tags) 
        {
            if (latitude < -90 || latitude > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(latitude), $"Latitude must be within the range -90 to 90.");
            }
            if (longitude < -180 || longitude > 180)
            {
                throw new ArgumentOutOfRangeException(nameof(longitude), $"Longitude must be within the range -180 to 180.");
            }

            Latitude = latitude;
            Longitude = longitude;
        }

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
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="latitude"/> is out of the range -90 to 90 or <paramref name="longitude"/> is out of the range -180 to 180.</exception>
        public Node(long id, bool visible, int version, long changeSet, DateTime timestamp, User user, double latitude, double longitude)
            : this(id, visible, version, changeSet, timestamp, user, latitude, longitude, tags: null) { }
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
        /// <param name="tags"> Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default.</param>
        /// <param name="latitude">Distance north or south of equator, must be within the range of -90 to 90.</param>
        /// <param name="longitude">Distance east or west of prime meridian, must be within the range of -180 to 180.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/>, <paramref name="version"/> or <paramref name="changeSet"/> is less than or equal to 0,
        /// or if <paramref name="latitude"/> is out of the range -90 to 90 or <paramref name="longitude"/> is out of the range -180 to 180.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="user"/> is <c>null</c>.</exception>
        [JsonConstructor]
        public Node(long id, bool visible, int version, long changeSet, DateTime timestamp, User user, double latitude, double longitude, Dictionary<string, string>? tags = null) 
            : base(id, visible, version, changeSet, timestamp, user, tags)
        {
            if (latitude < -90 || latitude > 90)
            {
                throw new ArgumentOutOfRangeException(nameof(latitude), $"Latitude must be within the range -90 to 90.");
            }
            if (longitude < -180 || longitude > 180)
            {
                throw new ArgumentOutOfRangeException(nameof(longitude), $"Longitude must be within the range -180 to 180.");
            }

            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>
        /// Checks if <see cref="Node"/> is part of a <see cref="Way"/> instance.
        /// </summary>
        /// <param name="way"><see cref="Way"/> instance to check if <see cref="Node"/> is part of.</param>
        /// <returns><c>true</c> if <see cref="Node"/> is part of <paramref name="way"/>. Otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="way"/> is <c>null</c>.</exception>
        public bool IsPartOf(Way way)
        {
            if (way == null)
                throw new ArgumentNullException(nameof(way), "Way cannot be null, must be defined.");

            return way.NodeReferenceIds.Contains(Id);
        }

        /// <summary>
        /// Checks if <see cref="Node"/> is part of a <see cref="Relation"/> instance.
        /// </summary>
        /// <param name="relation"><see cref="Relation"/> to check if <see cref="Node"/> is part of.</param>
        /// <returns><c>true</c> if <see cref="Node"/> is part of <paramref name="relation"/>. Otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="relation"/> is <c>null</c>.</exception>
        public bool IsPartOf(Relation relation)
        {
            if (relation == null)
                throw new ArgumentNullException(nameof(relation), "Relation cannot be null, must be defined.");

            return relation.Members.Any(member => member.ReferenceId == Id);
        }

        /// <summary>
        /// Checks if <see cref="Node"/> is part of both a <see cref="Way"/> instance and a <see cref="Relation"/> instance.
        /// </summary>
        /// <param name="way"><see cref="Way"/> to check if <see cref="Node"/> is part of.</param>
        /// <param name="relation"><see cref="Relation"/> to check if <see cref="Node"/> is part of.</param>
        /// <returns><c>true</c> if <see cref="Node"/> is part of <paramref name="way"/> and <paramref name="relation"/>. Otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="way"/> or <paramref name="relation"/> is <c>null</c>.</exception>
        public bool IsPartOf(Way way, Relation relation)
        {
            if (way == null)
                throw new ArgumentNullException(nameof(way), "Way cannot be null, must be defined.");

            if (relation == null)
                throw new ArgumentNullException(nameof(relation), "Relation cannot be null, must be defined.");

            return IsPartOf(way) && IsPartOf(relation);
        }
    }
}