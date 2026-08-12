using System.Text.Json.Serialization;

namespace OsmToolkit
{
    /// <summary>
    /// Contains user data about OSM-creators.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for <see cref="User"/>.
        /// </summary>
        [JsonPropertyName("id"), JsonPropertyOrder(0)]
        public long Id { get; }
        /// <summary>
        /// Name of the <see cref="User"/>.
        /// </summary>
        [JsonPropertyName("name"), JsonPropertyOrder(1)]
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new <see cref="User"/> object.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="User"/>.</param>
        /// <param name="name">Name of the <see cref="User"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0.</exception>
        /// <exception cref="ArgumentException">Throws if <paramref name="name"/> is <c>null</c> or an empty string.</exception>
        public User(long id, string name)
        {
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(id), $"Id must be greater than 0.");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"Name cannot be null or an empty string.", nameof(name));
            }

            Id = id;
            Name = name;
        }
    }
}
