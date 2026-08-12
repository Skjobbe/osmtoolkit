using System.Text.Json.Serialization;
namespace OsmToolkit
{
    /// <summary>
    /// Represents a map element.
    /// </summary>
    public abstract class OsmEntity
    {
        private Dictionary<string, string> _tags = new();
        /// <summary>
        /// A read-only dictionary of key-value string pairs representing metadata for <see cref="OsmEntity"/>.
        /// </summary>
        [JsonPropertyName("tags"), JsonPropertyOrder(8)]
        public IReadOnlyDictionary<string, string> Tags => _tags;

        /// <summary>
        /// Unique identifier for <see cref="OsmEntity"/>.
        /// </summary>
        [JsonPropertyName("id"), JsonPropertyOrder(0)]
        public long Id { get; }
        /// <summary>
        /// Indicates whether the <see cref="OsmEntity"/> is currently visible or active,
        /// <c>true</c> if it is still part of the active map data, or <c>false</c> if otherwise.
        /// </summary>
        [JsonPropertyName("visible"), JsonPropertyOrder(1)]
        public bool Visible { get; set; } = true;
        /// <summary>
        /// Current version number of <see cref="OsmEntity"/>.
        /// </summary>
        [JsonPropertyName("version"), JsonPropertyOrder(2)]
        public int Version { get; set; } = -1;
        /// <summary>
        /// Id of the set of changes the <see cref="OsmEntity"/> was last modified in.
        /// </summary>
        [JsonPropertyName("changeset"), JsonPropertyOrder(3)]
        public long ChangeSet { get; set; } = -1;
        /// <summary>
        /// Last time <see cref="OsmEntity"/> was changed.
        /// </summary>
        [JsonPropertyName("timestamp"), JsonPropertyOrder(4)]
        public DateTime Timestamp { get; set; } = DateTime.Now;
        /// <summary>
        /// Creator of <see cref="OsmEntity"/>.
        /// </summary>
        [JsonPropertyName("user"), JsonPropertyOrder(5)]
        public User? User { get; set; } = null;

        /// <summary>
        /// Initializes a new default <see cref="OsmEntity"/> object with just a specified id.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>, must be of values above 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0.</exception>
        protected OsmEntity(long id) 
        {
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(id), $"Id must be greater than 0.");
            }
            Id = id;
        }

        /// <summary>
        /// Intiliazes a new default <see cref="OsmEntity"/> object with just a specifed id and tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>, must be of values above 0.</param>
        /// <param name="tags">Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0.</exception>
        protected OsmEntity(long id, Dictionary<string, string>? tags = null) : this(id, true, -1, -1, DateTime.Now, null, tags) { }

        /// <summary>
        /// Initializes a new <see cref="OsmEntity"/> object with no tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>, must be of values above 0.</param>
        /// <param name="visible">Indicates whether the <see cref="OsmEntity"/> is currently visible or active,
        /// <c>true</c> if it is still part of the active map data, or <c>false</c> if otherwise.</param>
        /// <param name="version">Current version number of <see cref="OsmEntity"/>.</param>
        /// <param name="changeSet">The id of the set of changes the <see cref="OsmEntity"/> was modified in.</param>
        /// <param name="timestamp">Last time <see cref="OsmEntity"/> was changed.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0.</exception>
        protected OsmEntity(long id, bool visible, int version, long changeSet, DateTime timestamp) : this(id)
        {
            Visible = visible;
            Version = version;
            ChangeSet = changeSet;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Initializes a new <see cref="OsmEntity"/> object with tags.
        /// </summary>
        /// <param name="id">Unique identifier for <see cref="OsmEntity"/>, must be of values above 0.</param>
        /// <param name="visible">Indicates whether the <see cref="OsmEntity"/> is currently visible or active.
        /// <c>true</c> if it is still part of the active map data, or <c>false</c> if otherwise.</param>
        /// <param name="version">Current version number of <see cref="OsmEntity"/>.</param>
        /// <param name="changeSet">The id of the set of changes the <see cref="OsmEntity"/> was modified in.</param>
        /// <param name="timestamp">Last time <see cref="OsmEntity"/> was changed.</param>
        /// <param name="user">Creator of <see cref="OsmEntity"/>, can be <c>null</c>.</param>
        /// <param name="tags">Optional dictionary containing key-value string pairs used to attach metadata to the <see cref="OsmEntity"/>. If not provided or <c>null</c>, an empty dictionary is used by default.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="id"/> is less than or equal to 0.</exception>
        protected OsmEntity(long id, bool visible, int version, long changeSet, DateTime timestamp, User? user = null, Dictionary<string, string>? tags = null)
            : this(id, visible, version, changeSet, timestamp)
        {
            User = user;
            _tags = tags ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Adds or updates a dictionary containing metadata for <see cref="OsmEntity"/>.
        /// </summary>
        /// <param name="key">The key value to access the corresponding metadata, cannot be <c>null</c>  or an empty string.</param>
        /// <param name="value">The value describing the corresponding metadata, if <c>null</c> it will be set as an empty string.</param>
        /// <exception cref="ArgumentException">Throws if <paramref name="key"/> is <c>null</c> or an empty string.</exception>
        public void AddTag(string key, string value)
        {
            if(string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"{key} cannot be null or empty string.", nameof(key));

            _tags[key] = value ?? string.Empty;
        }

        /// <summary>
        /// Removes a single dictionary containing metadata for <see cref="OsmEntity"/>.
        /// </summary>
        /// <param name="key">The key value to access the corresponding metadata, cannot be <c>null</c> or an empty string.</param>
        /// <returns><c>true</c> if removal is successful; otherwise, <c>false</c>.</returns>
        public bool RemoveTag(string key)
        {
            return _tags.Remove(key);
        }

        /// <summary>
        /// Checks if the <see cref="OsmEntity"/> has a tag with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key to look for.</param>
        /// <returns><c>true</c>, if a tag with <paramref name="key"/> was found. Otherwise; <c>false</c>.</returns>
        public bool HasTagKey(string key)
        {
            return _tags.ContainsKey(key);
        }

        /// <summary>
        /// Checks if the <see cref="OsmEntity"/> has a tag with any key of the specified <paramref name="keys"/>.
        /// </summary>
        /// <param name="keys">Keys to look for.</param>
        /// <returns><c>true</c>, if a tag with any of the <paramref name="keys"/> was found. Otherwise; <c>false</c>.</returns>
        public bool HasAnyTagKey(IEnumerable<string> keys)
        {
            return keys.Any(_tags.ContainsKey);
        }

        /// <summary>
        /// Checks if the <see cref="OsmEntity"/> has tags with all of the specified <paramref name="keys"/>.
        /// </summary>
        /// <param name="keys">Keys to look for.</param>
        /// <returns><c>true</c>, if tags with all of the <paramref name="keys"/> were found. Otherwise; <c>false</c>.</returns>
        public bool HasAllTagKeys(IEnumerable<string> keys)
        {
            return keys.All(_tags.ContainsKey);
        }

        /// <summary>
        /// Checks if the <see cref="OsmEntity"/> has a tag with the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Value to look for.</param>
        /// <returns><c>true</c>, if a tag with <paramref name="value"/> was found. Otherwise; <c>false</c>.</returns>
        public bool HasTagValue(string value)
        {
            return _tags.ContainsValue(value);
        }

        /// <summary>
        /// Checks if the <see cref="OsmEntity"/> has a tag with any value of the specified <paramref name="values"/>.
        /// </summary>
        /// <param name="values">Values to look for.</param>
        /// <returns><c>true</c>, if a tag with any of the <paramref name="values"/> was found. Otherwise; <c>false</c>.</returns>
        public bool HasAnyTagValue(IEnumerable<string> values)
        {
            return values.Any(_tags.ContainsValue);
        }

        /// <summary>
        /// Checks if the <see cref="OsmEntity"/> has tags with all of the specified <paramref name="values"/>.
        /// </summary>
        /// <param name="values">Values to look for.</param>
        /// <returns><c>true</c>, if tags with all of the <paramref name="values"/> were found. Otherwise; <c>false</c>.</returns>
        public bool HasAllTagValues(IEnumerable<string> values)
        {
            return values.All(_tags.ContainsValue);
        }

        /// <summary>
        /// Checks if the <see cref="OsmEntity"/> has a tag matching the specified key and value.
        /// </summary>
        /// <param name="key">Key to look for.</param>
        /// <param name="value">Value to look for.</param>
        /// <returns><c>true</c>, if a tag with matches with <paramref name="key"/> and <paramref name="value"/> was found. Otherwise; <c>false</c>.</returns>
        public bool HasTag(string key, string value)
        {
            return _tags.ContainsKey(key) && _tags[key] == value;
        }

        /// <summary>
        /// Checks if the <see cref="OsmEntity"/> has any tag matching one the specified <paramref name="tags"/>.
        /// </summary>
        /// <param name="tags">Tags to look for.</param>
        /// <returns><c>true</c>, if a tag with matches one of the <paramref name="tags"/> was found. Otherwise; <c>false</c>.</returns>
        public bool HasAnyTag(params (string key, string value)[] tags)
        {
            return tags.Any(tag => _tags.TryGetValue(tag.key, out var val) && val == tag.value);
        }

        /// <summary>
        /// Checks if the <see cref="OsmEntity"/> has any tag matching one the specified <paramref name="tags"/>.
        /// </summary>
        /// <param name="tags">Tags to look for.</param>
        /// <returns><c>true</c>, if a tag with matches one of the <paramref name="tags"/> was found. Otherwise; <c>false</c>.</returns>
        public bool HasAnyTag(Dictionary<string, string> tags)
        {
            return tags.Any(tag => _tags.TryGetValue(tag.Key, out var val) && val == tag.Value);
        }

        /// <summary>
        /// Checks if the <see cref="OsmEntity"/> has any tag matching one the specified <paramref name="tags"/>.
        /// </summary>
        /// <param name="tags">Tags to look for.</param>
        /// <returns><c>true</c>, if a tag with matches one of the <paramref name="tags"/> was found. Otherwise; <c>false</c>.</returns>
        public bool HasAnyTag(IEnumerable<KeyValuePair<string, string>> tags)
        {
            return tags.Any(tag => _tags.TryGetValue(tag.Key, out var val) && val == tag.Value);
        }

        /// <summary>
        /// Checks if the <see cref="OsmEntity"/> has tags matching all of the specified <paramref name="tags"/>.
        /// </summary>
        /// <param name="tags">Tags to look for.</param>
        /// <returns><c>true</c>, if tags matching all of the <paramref name="tags"/> were found. Otherwise; <c>false</c>.</returns>
        public bool HasAllTags(params (string key, string value)[] tags)
        {
            return tags.All(tag => _tags.TryGetValue(tag.key, out var val) && val == tag.value);
        }

        /// <summary>
        /// Checks if the <see cref="OsmEntity"/> has tags matching all of the specified <paramref name="tags"/>.
        /// </summary>
        /// <param name="tags">Tags to look for.</param>
        /// <returns><c>true</c>, if tags matching all of the <paramref name="tags"/> were found. Otherwise; <c>false</c>.</returns>
        public bool HasAllTags(Dictionary<string, string> tags)
        {
            return tags.All(tag => _tags.TryGetValue(tag.Key, out var val) && val == tag.Value);
        }

        /// <summary>
        /// Checks if the <see cref="OsmEntity"/> has tags matching all of the specified <paramref name="tags"/>.
        /// </summary>
        /// <param name="tags">Tags to look for.</param>
        /// <returns><c>true</c>, if tags matching all of the <paramref name="tags"/> were found. Otherwise; <c>false</c>.</returns>
        public bool HasAllTags(IEnumerable<KeyValuePair<string, string>> tags)
        {
            return tags.All(tag => _tags.TryGetValue(tag.Key, out var val) && val == tag.Value);
        }

        internal string ToTagString(bool multiline = false)
        {
            if(_tags.Count == 0)
            {
                return string.Empty;
            }

            var seperator = multiline ? Environment.NewLine : ", ";
            return string.Join(seperator, _tags.Select(kv => $"{kv.Key}={kv.Value}"));
        }
    }
}
