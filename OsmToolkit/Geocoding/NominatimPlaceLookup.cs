using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OsmToolkit.Geocoding.Logging;
using System.Globalization;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OsmToolkit.Geocoding
{
    /// <summary>
    /// Resolves a free-text place name to a geographic location live from Nominatim over HTTP.
    /// Repeated lookups for the same place name are served from an in-memory cache.
    /// </summary>
    internal class NominatimPlaceLookup : IPlaceLookup
    {
        /// <summary>
        /// The public Nominatim search endpoint used when no endpoint override is supplied.
        /// </summary>
        internal const string DefaultEndpoint = "https://nominatim.openstreetmap.org/search";
        /// <summary>
        /// The default duration, in hours, a resolved place is retained in the in-memory cache before expiring.
        /// </summary>
        internal const int DefaultCacheDurationHours = 24;
        /// <summary>
        /// The default total size limit of the in-memory cache, in entries.
        /// </summary>
        internal const long DefaultCacheSizeLimit = 1_000L;
        /// <summary>
        /// The default minimum interval enforced between successive requests, per Nominatim's usage policy.
        /// </summary>
        internal static readonly TimeSpan DefaultMinimumRequestInterval = TimeSpan.FromSeconds(1);

        private const string RepositoryUrl = "https://github.com/Skjobbe/osmtoolkit";
        private const string ProductName = "OsmToolkit";

        private static readonly HttpClient SharedHttpClient = new();
        private static readonly IMemoryCache SharedCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = DefaultCacheSizeLimit });
        private static readonly RateGate SharedRateGate = new(DefaultMinimumRequestInterval);
        private static readonly string ProductVersion = ResolveProductVersion();
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Test seam allowing the internally-owned default <see cref="HttpClient"/> to be swapped out for a fake handler.
        /// Not intended for use by consumers; only visible within this assembly and to <c>OsmToolkitTests</c>.
        /// </summary>
        internal static HttpClient? DefaultHttpClientOverride { get; set; }

        /// <summary>
        /// Test seam allowing the internally-owned default <see cref="IMemoryCache"/> to be swapped out for a fresh instance per test.
        /// Not intended for use by consumers; only visible within this assembly and to <c>OsmToolkitTests</c>.
        /// </summary>
        internal static IMemoryCache? DefaultCacheOverride { get; set; }

        /// <summary>
        /// Test seam allowing the internally-owned default <see cref="RateGate"/> to be swapped out, so tests can
        /// observe gating behavior without incurring a real 1-second wait per request.
        /// Not intended for use by consumers; only visible within this assembly and to <c>OsmToolkitTests</c>.
        /// </summary>
        internal static RateGate? DefaultRateGateOverride { get; set; }

        private readonly HttpClient _httpClient;
        private readonly ILogger<NominatimPlaceLookup> _logger;
        private readonly string _endpoint;
        private readonly TimeSpan _cacheDuration;
        private readonly IMemoryCache _cache;
        private readonly RateGate _rateGate;

        /// <summary>
        /// Initializes a new instance of the <see cref="NominatimPlaceLookup"/> class.
        /// </summary>
        /// <param name="httpClient">An optional <see cref="HttpClient"/> to use for requests. If not provided, an internally-owned shared instance is used.</param>
        /// <param name="logger">An optional logger for diagnostics. If not provided, a <see cref="NullLogger{NominatimPlaceLookup}"/> is used.</param>
        /// <param name="endpoint">An optional Nominatim search endpoint override. If not provided, the public Nominatim instance is used.</param>
        /// <param name="cacheDuration">How long a resolved place is retained in the in-memory cache before expiring. If not provided, defaults to 24 hours.</param>
        /// <param name="cache">An optional <see cref="IMemoryCache"/> used to cache resolved places, typically the singleton registered by <c>AddOsmToolkit()</c>. If not provided, an internally-owned shared instance is used.</param>
        public NominatimPlaceLookup(
            HttpClient? httpClient = null,
            ILogger<NominatimPlaceLookup>? logger = null,
            string? endpoint = null,
            TimeSpan? cacheDuration = null,
            IMemoryCache? cache = null)
        {
            _httpClient = httpClient ?? DefaultHttpClientOverride ?? SharedHttpClient;
            _logger = logger ?? new NullLogger<NominatimPlaceLookup>();
            _endpoint = string.IsNullOrWhiteSpace(endpoint) ? DefaultEndpoint : endpoint;
            _cacheDuration = cacheDuration ?? TimeSpan.FromHours(DefaultCacheDurationHours);
            _cache = cache ?? DefaultCacheOverride ?? SharedCache;
            _rateGate = DefaultRateGateOverride ?? SharedRateGate;
        }

        /// <inheritdoc />
        public async Task<PlaceLookupResult> FindAsync(string placeName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(placeName))
                throw new ArgumentException("Place name cannot be null or empty.", nameof(placeName));

            if (_cache.TryGetValue(placeName, out PlaceLookupResult? cached) && cached is not null)
            {
                GeocodingLogMessages.LogCacheHit(_logger, placeName);
                return cached;
            }

            await _rateGate.WaitForTurnAsync(cancellationToken);

            var result = await FetchAsync(placeName, cancellationToken);

            _cache.Set(placeName, result, new MemoryCacheEntryOptions
            {
                Size = 1,
                AbsoluteExpirationRelativeToNow = _cacheDuration
            });

            return result;
        }

        private async Task<PlaceLookupResult> FetchAsync(string placeName, CancellationToken cancellationToken)
        {
            var url = $"{_endpoint}?format=json&limit=1&q={Uri.EscapeDataString(placeName)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue(ProductName, ProductVersion));
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue($"(+{RepositoryUrl})"));

            GeocodingLogMessages.LogFetchStart(_logger, placeName, _endpoint);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                GeocodingLogMessages.LogFetchFailed(_logger, response.StatusCode);
            }
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            NominatimResult[]? results;
            try
            {
                results = JsonSerializer.Deserialize<NominatimResult[]>(body, JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Nominatim returned a response that could not be parsed as JSON.", ex);
            }

            if (results is null || results.Length == 0)
            {
                GeocodingLogMessages.LogNoMatch(_logger, placeName);
                throw new PlaceNotFoundException(placeName);
            }

            var result = ToPlaceLookupResult(placeName, results[0]);

            GeocodingLogMessages.LogFetchResult(_logger, placeName, result.Latitude, result.Longitude);

            return result;
        }

        private static PlaceLookupResult ToPlaceLookupResult(string placeName, NominatimResult match)
        {
            if (match.BoundingBox is not { Length: 4 } boundingBox ||
                match.Latitude is null || match.Longitude is null || match.DisplayName is null)
            {
                throw new InvalidOperationException($"Nominatim returned an incomplete result for \"{placeName}\".");
            }

            // Nominatim orders its bounding box as [min lat, max lat, min lon, max lon] — not the
            // (minLat, minLon, maxLat, maxLon) order OsmCoordinateBounds' constructor expects.
            var minLat = double.Parse(boundingBox[0], CultureInfo.InvariantCulture);
            var maxLat = double.Parse(boundingBox[1], CultureInfo.InvariantCulture);
            var minLon = double.Parse(boundingBox[2], CultureInfo.InvariantCulture);
            var maxLon = double.Parse(boundingBox[3], CultureInfo.InvariantCulture);

            var latitude = double.Parse(match.Latitude, CultureInfo.InvariantCulture);
            var longitude = double.Parse(match.Longitude, CultureInfo.InvariantCulture);

            var bounds = new OsmCoordinateBounds(minLat, minLon, maxLat, maxLon);

            return new PlaceLookupResult(match.DisplayName, latitude, longitude, bounds);
        }

        private static string ResolveProductVersion()
        {
            var informationalVersion = typeof(NominatimPlaceLookup).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            if (string.IsNullOrWhiteSpace(informationalVersion))
                return "0.0.0";

            var plusIndex = informationalVersion.IndexOf('+');
            return plusIndex >= 0 ? informationalVersion[..plusIndex] : informationalVersion;
        }

        private sealed class NominatimResult
        {
            [JsonPropertyName("display_name")]
            public string? DisplayName { get; set; }
            [JsonPropertyName("lat")]
            public string? Latitude { get; set; }
            [JsonPropertyName("lon")]
            public string? Longitude { get; set; }
            [JsonPropertyName("boundingbox")]
            public string[]? BoundingBox { get; set; }
        }
    }
}
