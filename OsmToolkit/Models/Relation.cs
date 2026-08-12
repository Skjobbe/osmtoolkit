using System.Text.Json.Serialization;
namespace OsmToolkit
{
    /// <summary>
    /// Representation of a geographic feature.
    /// </summary>
    public class Relation : OsmEntity
    {
        private readonly List<Member> _members = new();
        /// <summary>
        /// List of the <see cref="Relation"/> instance's members.
        /// </summary>
        [JsonPropertyName("members"), JsonPropertyOrder(6)]
        public IList<Member> Members => _members;

        /// <summary>
        /// Initializes a new default <see cref="Relation"/> object without specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="members">Collection of the <see cref="Relation"/>'s members.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="members"/> contain less than one element.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="members"/> is <c>null</c>.</exception>
        public Relation(long id, IList<Member> members) : this(id, members, null) { }

        /// <summary>
        /// Initializes a new default <see cref="Relation"/> object with specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="members">Collection of the <see cref="Relation"/>'s members.</param>
        /// <param name="tags">Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="members"/> contain less than one element.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="members"/> is <c>null</c>.</exception>
        public Relation(long id, IList<Member> members, Dictionary<string, string>? tags = null)
            : base(id, tags)
        {
            ValidationCheck(members);
        }

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
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="members"/> contain less than one element.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="members"/> is <c>null</c>.</exception>
        public Relation(long id, bool visible, int version, long changeSet, DateTime timestamp, User user, IList<Member> members)
            : this(id, visible, version, changeSet, timestamp, user, members, tags: null) { }

        /// <summary>
        /// Initializes a new <see cref="Relation"/> object with specified tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>.</param>
        /// <param name="visible">Indicates whether the <see cref="OsmEntity"/> is currently visible or active.
        /// <c>true</c> if it is still part of the active map data, or <c>false</c> if otherwise.</param>
        /// <param name="version">Current version number of <see cref="OsmEntity"/>.</param>
        /// <param name="changeSet">The id of the set of changes the <see cref="OsmEntity"/> was modified in.</param>
        /// <param name="timestamp">Last time <see cref="OsmEntity"/> was changed.</param>
        /// <param name="user">Creator of <see cref="OsmEntity"/>, can be <c>null</c>..</param>
        /// <param name="tags">Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default.</param>
        /// <param name="members">Collection of the <see cref="Relation"/>'s members.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0,
        /// or if <paramref name="members"/> contain less than one element.</exception>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="members"/> is <c>null</c>.</exception>
        public Relation(long id, bool visible, int version, long changeSet, DateTime timestamp, User user, IList<Member> members, Dictionary<string, string>? tags = null)
            : base(id, visible, version, changeSet, timestamp, user, tags)
        {
            ValidationCheck(members);
        }
        private void ValidationCheck(IList<Member> members)
        {
            if (members is null)
            {
                throw new ArgumentNullException(nameof(members), $"Members cannot be null.");
            }
            if (members.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(members), $"Members must have at least one element.");
            }

            _members.AddRange(members);
        }
    }
}
