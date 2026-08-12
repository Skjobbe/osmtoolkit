using System.Text.Json.Serialization;

namespace OsmToolkit
{
    /// <summary>
    /// Represents the header of the OSM-data, describing where it was generated and information about copyright.
    /// </summary>
    public class OsmHeader
    {
        /// <summary>
        /// Current version number of the OSM schema.
        /// </summary>
        [JsonPropertyName("version"), JsonPropertyOrder(0)]
        public double Version { get; }
        /// <summary>
        /// Describes the generator used to create the OSM-data.
        /// </summary>
        [JsonPropertyName("generator"), JsonPropertyOrder(1)]
        public string Generator { get; } = string.Empty;
        /// <summary>
        /// Describes the copyright owners of the OSM-data.
        /// </summary>
        [JsonPropertyName("copyright"), JsonPropertyOrder(2)]
        public string Copyright { get; } = string.Empty;
        /// <summary>
        /// Contains an URL to the copyright owners' copyright page.
        /// </summary>
        [JsonPropertyName("attribution"), JsonPropertyOrder(3)]
        public string AttributionUrl { get; } = string.Empty;
        /// <summary>
        /// Contains an URL to the copyright owners' license of use.
        /// </summary>5
        [JsonPropertyName("license"), JsonPropertyOrder(4)]
        public string LicenseUrl { get; } = string.Empty;

        /// <summary>
        /// Initiliazes a new default <see cref="OsmHeader"/> with version.
        /// </summary>
        /// <param name="version">Current version number of the OSM-data. Cannot be less than or equal to 0</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="version"/> is less than or equal to 0.</exception>
        public OsmHeader(double version)
        {
            if (version <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(version), $"Version must be greater than 0.");
            }

            Version = version;
        }
        /// <summary>
        /// Initiliazes a new <see cref="OsmHeader"/> object.
        /// </summary>
        /// <param name="version">Current version number of the OSM-data. Cannot be less than or equal to 0</param>
        /// <param name="generator">Generator used to create the OSM-data. If <c>null</c>, it will be set to <c>""</c>.</param>
        /// <param name="copyright">Copyright owners of the OSM-data. If <c>null</c>, it will be set to <c>""</c>.</param>
        /// <param name="attributionUrl">URL to the copyright owners' copyright page. If <c>null</c>, it will be set to <c>""</c>.</param>
        /// <param name="licenseUrl">URL to the copyright owners' license of use. If <c>null</c>, it will be set to <c>""</c>.</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="version"/> is less than or equal to 0.</exception>
        public OsmHeader(double version, string? generator, string? copyright, string? attributionUrl, string? licenseUrl) : this(version)
        {
            Generator = generator ?? string.Empty;
            Copyright = copyright ?? string.Empty;
            AttributionUrl = attributionUrl ?? string.Empty;
            LicenseUrl = licenseUrl ?? string.Empty;
        }
        
    }
}
