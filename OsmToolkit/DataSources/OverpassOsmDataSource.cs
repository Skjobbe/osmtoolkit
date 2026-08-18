using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OsmToolkit.DataSources.Logging;
using OsmToolkit.Serialization.Json;
using System.Globalization;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace OsmToolkit.DataSources
{
    /// <summary>
    /// Fetches OSM data for a bounding box live from the Overpass API over HTTP.
    /// Repeated requests for the same bounds are served from a private, internally-owned in-memory cache.
    /// </summary>
    internal class OverpassOsmDataSource : IOsmDataSource, IDisposable
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
        /// <summary>
        /// The default duration, in minutes, a fetched result is retained in the in-memory cache before expiring.
        /// </summary>
        internal const int DefaultCacheDurationMinutes = 15;
        /// <summary>
        /// The default total size limit of the in-memory cache, in weighted OSM elements (nodes + ways + relations).
        /// </summary>
        internal const long DefaultCacheSizeLimit = 200_000L;

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
        private readonly TimeSpan _cacheDuration;
        private readonly MemoryCache _cache;

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
        /// <param name="cacheDuration">How long a fetched result is retained in the in-memory cache before expiring. If not provided, defaults to 15 minutes.</param>
        /// <param name="cacheSizeLimit">The total size limit of the in-memory cache, in weighted OSM elements (nodes + ways + relations). Defaults to 200,000.</param>
        public OverpassOsmDataSource(
            HttpClient? httpClient = null,
            IOsmJsonDeserializer? deserializer = null,
            ILogger<OverpassOsmDataSource>? logger = null,
            string? endpoint = null,
            int queryTimeoutSeconds = DefaultQueryTimeoutSeconds,
            long queryMaxSizeBytes = DefaultQueryMaxSizeBytes,
            double maxAreaSquareKilometers = DefaultMaxAreaSquareKilometers,
            TimeSpan? cacheDuration = null,
            long cacheSizeLimit = DefaultCacheSizeLimit)
        {
            _httpClient = httpClient ?? DefaultHttpClientOverride ?? SharedHttpClient;
            _deserializer = deserializer ?? new OsmJsonDeserializer();
            _logger = logger ?? new NullLogger<OverpassOsmDataSource>();
            _endpoint = string.IsNullOrWhiteSpace(endpoint) ? DefaultEndpoint : endpoint;
            _queryTimeoutSeconds = queryTimeoutSeconds;
            _queryMaxSizeBytes = queryMaxSizeBytes;
            _maxAreaSquareKilometers = maxAreaSquareKilometers;
            _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(DefaultCacheDurationMinutes);
            _cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = cacheSizeLimit });
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

            var cacheKey = (bounds.MinimumLatitude, bounds.MinimumLongitude, bounds.MaximumLatitude, bounds.MaximumLongitude);
            if (_cache.TryGetValue(cacheKey, out OsmData? cachedData) && cachedData is not null)
            {
                DataSourceLogMessages.LogCacheHit(_logger, bounds.MinimumLatitude, bounds.MinimumLongitude, bounds.MaximumLatitude, bounds.MaximumLongitude);
                return cachedData;
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

            var remark = TryGetRemark(body);
            if (remark is not null)
            {
                DataSourceLogMessages.LogRemarkDetected(_logger, remark);
                throw new InvalidOperationException(remark);
            }

            var data = await _deserializer.DeserializeAsync(body, cancellationToken);

            DataSourceLogMessages.LogFetchResult(_logger, data.Nodes.Count, data.Ways.Count, data.Relations.Count);

            var weight = data.Nodes.Count + data.Ways.Count + data.Relations.Count;
            _cache.Set(cacheKey, data, new MemoryCacheEntryOptions
            {
                Size = weight,
                AbsoluteExpirationRelativeToNow = _cacheDuration
            });

            return data;
        }

        /// <summary>
        /// Releases the internally-owned in-memory cache.
        /// </summary>
        public void Dispose()
        {
            _cache.Dispose();
            GC.SuppressFinalize(this);
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

        /// <summary>
        /// Checks the raw Overpass response body for a top-level <c>remark</c> field, which Overpass sets on an HTTP 200
        /// response when the query failed server-side (e.g. hit the execution timeout or memory ceiling).
        /// </summary>
        private static string? TryGetRemark(string body)
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.TryGetProperty("remark", out var remarkElement) && remarkElement.ValueKind == JsonValueKind.String
                ? remarkElement.GetString()
                : null;
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
