using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OsmToolkit.DataSources.Logging;
using OsmToolkit.Serialization.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Reflection;

namespace OsmToolkit.DataSources
{
    /// <summary>
    /// Fetches OSM data for a bounding box live from the Overpass API over HTTP.
    /// </summary>
    internal class OverpassOsmDataSource : IOsmDataSource
    {
        /// <summary>
        /// The public Overpass API interpreter endpoint used when no endpoint override is supplied.
        /// </summary>
        internal const string DefaultEndpoint = "https://overpass-api.de/api/interpreter";
        /// <summary>
        /// The default server-side Overpass query execution timeout, in seconds.
        /// </summary>
        internal const int DefaultQueryTimeoutSeconds = 25;
        /// <summary>
        /// The default server-side Overpass query memory ceiling, in bytes (1 GiB).
        /// </summary>
        internal const long DefaultQueryMaxSizeBytes = 1_073_741_824L;
        /// <summary>
        /// The default ceiling on a requested bounding box's estimated area, in square kilometers.
        /// </summary>
        internal const double DefaultMaxAreaSquareKilometers = 10_000d;

        private const string RepositoryUrl = "https://github.com/Skjobbe/osmtoolkit";
        private const string ProductName = "OsmToolkit";
        private const double KilometersPerDegreeLatitude = 111.32d;

        private static readonly HttpClient SharedHttpClient = new();
        private static readonly string ProductVersion = ResolveProductVersion();

        /// <summary>
        /// Test seam allowing the internally-owned default <see cref="HttpClient"/> to be swapped out for a fake handler.
        /// Not intended for use by consumers; only visible within this assembly and to <c>OsmToolkitTests</c>.
        /// </summary>
        internal static HttpClient? DefaultHttpClientOverride { get; set; }

        private readonly HttpClient _httpClient;
        private readonly IOsmJsonDeserializer _deserializer;
        private readonly ILogger<OverpassOsmDataSource> _logger;
        private readonly string _endpoint;
        private readonly int _queryTimeoutSeconds;
        private readonly long _queryMaxSizeBytes;
        private readonly double _maxAreaSquareKilometers;

        /// <summary>
        /// Initializes a new instance of the <see cref="OverpassOsmDataSource"/> class.
        /// </summary>
        /// <param name="httpClient">An optional <see cref="HttpClient"/> to use for requests. If not provided, an internally-owned shared instance is used.</param>
        /// <param name="deserializer">An optional deserializer used to parse the Overpass response. If not provided, a default <see cref="OsmJsonDeserializer"/> is used.</param>
        /// <param name="logger">An optional logger for diagnostics. If not provided, a <see cref="NullLogger{OverpassOsmDataSource}"/> is used.</param>
        /// <param name="endpoint">An optional Overpass interpreter endpoint override. If not provided, the public Overpass API interpreter endpoint is used.</param>
        /// <param name="queryTimeoutSeconds">The server-side Overpass query execution timeout, in seconds. Defaults to 25 seconds.</param>
        /// <param name="queryMaxSizeBytes">The server-side Overpass query memory ceiling, in bytes. Defaults to 1 GiB.</param>
        /// <param name="maxAreaSquareKilometers">The ceiling on a requested bounding box's estimated area, in square kilometers, above which a request is rejected before any network call. Defaults to 10,000 km².</param>
        public OverpassOsmDataSource(
            HttpClient? httpClient = null,
            IOsmJsonDeserializer? deserializer = null,
            ILogger<OverpassOsmDataSource>? logger = null,
            string? endpoint = null,
            int queryTimeoutSeconds = DefaultQueryTimeoutSeconds,
            long queryMaxSizeBytes = DefaultQueryMaxSizeBytes,
            double maxAreaSquareKilometers = DefaultMaxAreaSquareKilometers)
        {
            _httpClient = httpClient ?? DefaultHttpClientOverride ?? SharedHttpClient;
            _deserializer = deserializer ?? new OsmJsonDeserializer();
            _logger = logger ?? new NullLogger<OverpassOsmDataSource>();
            _endpoint = string.IsNullOrWhiteSpace(endpoint) ? DefaultEndpoint : endpoint;
            _queryTimeoutSeconds = queryTimeoutSeconds;
            _queryMaxSizeBytes = queryMaxSizeBytes;
            _maxAreaSquareKilometers = maxAreaSquareKilometers;
        }

        /// <inheritdoc />
        public async Task<OsmData> GetOsmDataAsync(OsmCoordinateBounds bounds, CancellationToken cancellationToken = default)
        {
            if (bounds is null)
                throw new ArgumentNullException(nameof(bounds), "Bounds cannot be null, must be defined.");

            var areaSquareKilometers = EstimateAreaSquareKilometers(bounds);
            if (areaSquareKilometers > _maxAreaSquareKilometers)
            {
                DataSourceLogMessages.LogAreaRejected(_logger, areaSquareKilometers, _maxAreaSquareKilometers);
                throw new ArgumentOutOfRangeException(nameof(bounds), areaSquareKilometers,
                    $"Estimated area of {areaSquareKilometers:F0} km² exceeds the maximum allowed area of {_maxAreaSquareKilometers:F0} km².");
            }

            var query = BuildQuery(bounds);

            using var request = new HttpRequestMessage(HttpMethod.Post, _endpoint)
            {
                Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("data", query) })
            };
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(ProductName, ProductVersion));
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue($"(+{RepositoryUrl})"));

            DataSourceLogMessages.LogFetchStart(_logger, bounds.MinimumLatitude, bounds.MinimumLongitude, bounds.MaximumLatitude, bounds.MaximumLongitude, _endpoint);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                DataSourceLogMessages.LogFetchFailed(_logger, response.StatusCode);
            }
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var data = await _deserializer.DeserializeAsync(body, cancellationToken);

            DataSourceLogMessages.LogFetchResult(_logger, data.Nodes.Count, data.Ways.Count, data.Relations.Count);

            return data;
        }

        private string BuildQuery(OsmCoordinateBounds bounds)
        {
            var minLat = bounds.MinimumLatitude.ToString(CultureInfo.InvariantCulture);
            var minLon = bounds.MinimumLongitude.ToString(CultureInfo.InvariantCulture);
            var maxLat = bounds.MaximumLatitude.ToString(CultureInfo.InvariantCulture);
            var maxLon = bounds.MaximumLongitude.ToString(CultureInfo.InvariantCulture);

            return $"""
                [out:json][timeout:{_queryTimeoutSeconds}][maxsize:{_queryMaxSizeBytes}];
                (
                  node({minLat},{minLon},{maxLat},{maxLon});
                  way({minLat},{minLon},{maxLat},{maxLon});
                  relation({minLat},{minLon},{maxLat},{maxLon});
                );
                out body;
                >;
                out skel qt;
                """;
        }

        private static double EstimateAreaSquareKilometers(OsmCoordinateBounds bounds)
        {
            var averageLatitudeRadians = (bounds.MinimumLatitude + bounds.MaximumLatitude) / 2d * Math.PI / 180d;
            var heightKilometers = (bounds.MaximumLatitude - bounds.MinimumLatitude) * KilometersPerDegreeLatitude;
            var widthKilometers = (bounds.MaximumLongitude - bounds.MinimumLongitude) * KilometersPerDegreeLatitude * Math.Cos(averageLatitudeRadians);

            return heightKilometers * Math.Abs(widthKilometers);
        }

        private static string ResolveProductVersion()
        {
            var informationalVersion = typeof(OverpassOsmDataSource).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            if (string.IsNullOrWhiteSpace(informationalVersion))
                return "0.0.0";

            var plusIndex = informationalVersion.IndexOf('+');
            return plusIndex >= 0 ? informationalVersion[..plusIndex] : informationalVersion;
        }
    }
}
