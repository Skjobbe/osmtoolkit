using System.Text.Json.Serialization;

namespace OsmToolkit
{
    /// <summary>
    /// A <see cref="Member"/> within a <see cref="Relation"/> instance.
    /// </summary>
    public class Member
    {
        /// <summary>
        /// The specified <see cref="OsmEntity"/> type.
        /// </summary>
        [JsonPropertyName("type"), JsonPropertyOrder(0)]
        public ReferenceType Type { get; set; }
        /// <summary>
        /// A reference to the id of the <see cref="OsmEntity"/> instance.
        /// </summary>
        [JsonPropertyName("ref"), JsonPropertyOrder(1)]
        public long ReferenceId { get; }
        /// <summary>
        /// The specified <see cref="Member"/> role for a <see cref="Relation"/>.
        /// </summary>
        [JsonPropertyName("role"), JsonPropertyOrder(2)]
        public string Role { get; set; }

        /// <summary>
        /// Initializes a new <see cref="Member"/> object without a specified role.
        /// </summary>
        /// <param name="type">The specified <see cref="OsmEntity"/> type.</param>
        /// <param name="referenceId">The reference to the id of <see cref="OsmEntity"/>, must be of values above 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="referenceId"/> is less than or equal to 0.</exception>
        public Member(ReferenceType type, long referenceId)
        {
            if (referenceId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(referenceId), $"{referenceId} cannot be equal to or less than 0.");
            }

            Type = type;
            ReferenceId = referenceId;
            Role = string.Empty;
        }

        /// <summary>
        /// Initializes a new <see cref="Member"/> object with a specified role.
        /// </summary>
        /// <param name="type">The specified <see cref="OsmEntity"/> type.</param>
        /// <param name="referenceId">The reference to the id of <see cref="OsmEntity"/>, must be of values above 0.</param>
        /// <param name="role">The specified <see cref="Member"/> role, can be null.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="referenceId"/> is less than or equal to 0.</exception>
        public Member(ReferenceType type, long referenceId, string? role) : this(type, referenceId)
        {
            Role = role ?? string.Empty;
        }
    }
}
