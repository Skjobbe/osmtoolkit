using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OsmToolkit.DataSources.Logging;
using OsmToolkit.Serialization.Json;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace OsmToolkit.DataSources
{
    /// <summary>
    /// Fetches OSM data for a bounding box live from the Overpass API over HTTP.
    /// Repeated requests for the same bounds are served from an in-memory cache.
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
        /// <summary>
        /// The default duration, in minutes, a fetched result is retained in the in-memory cache before expiring.
        /// </summary>
        internal const int DefaultCacheDurationMinutes = 15;
        /// <summary>
        /// The default total size limit of the in-memory cache, in weighted OSM elements (nodes + ways + relations).
        /// </summary>
        internal const long DefaultCacheSizeLimit = 200_000L;
        /// <summary>
        /// The default number of additional attempts made after a transient Overpass failure before giving up.
        /// A value of 0 disables retry entirely, preserving the original fail-fast behavior.
        /// </summary>
        internal const int DefaultMaxRetryAttempts = 1;

        private const string RepositoryUrl = "https://github.com/Skjobbe/osmtoolkit";
        private const string ProductName = "OsmToolkit";
        private const double KilometersPerDegreeLatitude = 111.32d;
        private static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromSeconds(1);

        private static readonly HttpClient SharedHttpClient = new();
        private static readonly IMemoryCache SharedCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = DefaultCacheSizeLimit });
        private static readonly string ProductVersion = ResolveProductVersion();

        /// <summary>
        /// Tracks in-flight Overpass fetches per cache instance, so that concurrent requests for identical bounds
        /// that both land during a cache miss coalesce into a single outbound request instead of each issuing
        /// their own. Scoped by cache instance (via a <see cref="ConditionalWeakTable{TKey,TValue}"/>) rather than
        /// held as a single static dictionary, so it naturally mirrors the sharing/isolation of whichever cache
        /// instance a given source is using — the process-wide default, or a caller-supplied one.
        /// </summary>
        private static readonly ConditionalWeakTable<IMemoryCache, ConcurrentDictionary<CacheKey, Lazy<Task<OsmData>>>> InFlightFetchesByCache = new();

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
        /// Test seam allowing the delay between retry attempts to be shortened so retry tests don't have to wait
        /// out the real backoff. Not intended for use by consumers; only visible within this assembly and to
        /// <c>OsmToolkitTests</c>.
        /// </summary>
        internal static TimeSpan? RetryDelayOverride { get; set; }

        private readonly HttpClient _httpClient;
        private readonly IOsmJsonDeserializer _deserializer;
        private readonly ILogger<OverpassOsmDataSource> _logger;
        private readonly string _endpoint;
        private readonly int _queryTimeoutSeconds;
        private readonly long _queryMaxSizeBytes;
        private readonly double _maxAreaSquareKilometers;
        private readonly TimeSpan _cacheDuration;
        private readonly IMemoryCache _cache;
        private readonly int _maxRetryAttempts;

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
        /// <param name="cache">An optional <see cref="IMemoryCache"/> used to cache fetched results, typically the singleton registered by <c>AddOsmToolkit()</c>. If not provided, an internally-owned shared instance is used.</param>
        /// <param name="maxRetryAttempts">
        /// The number of additional attempts made after a transient Overpass failure (an HTTP 429/502/503/504 response,
        /// or a <see cref="OverpassQueryFailedException"/>) before giving up. Defaults to 1. Pass 0 to disable retry
        /// and fail fast on the first failure, regardless of its shape.
        /// </param>
        public OverpassOsmDataSource(
            HttpClient? httpClient = null,
            IOsmJsonDeserializer? deserializer = null,
            ILogger<OverpassOsmDataSource>? logger = null,
            string? endpoint = null,
            int queryTimeoutSeconds = DefaultQueryTimeoutSeconds,
            long queryMaxSizeBytes = DefaultQueryMaxSizeBytes,
            double maxAreaSquareKilometers = DefaultMaxAreaSquareKilometers,
            TimeSpan? cacheDuration = null,
            IMemoryCache? cache = null,
            int maxRetryAttempts = DefaultMaxRetryAttempts)
        {
            if (maxRetryAttempts < 0)
                throw new ArgumentOutOfRangeException(nameof(maxRetryAttempts), maxRetryAttempts, "Must be zero or greater.");

            _httpClient = httpClient ?? DefaultHttpClientOverride ?? SharedHttpClient;
            _deserializer = deserializer ?? new OsmJsonDeserializer();
            _logger = logger ?? new NullLogger<OverpassOsmDataSource>();
            _endpoint = string.IsNullOrWhiteSpace(endpoint) ? DefaultEndpoint : endpoint;
            _queryTimeoutSeconds = queryTimeoutSeconds;
            _queryMaxSizeBytes = queryMaxSizeBytes;
            _maxAreaSquareKilometers = maxAreaSquareKilometers;
            _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(DefaultCacheDurationMinutes);
            _cache = cache ?? DefaultCacheOverride ?? SharedCache;
            _maxRetryAttempts = maxRetryAttempts;
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

            var cacheKey = new CacheKey(bounds.MinimumLatitude, bounds.MinimumLongitude, bounds.MaximumLatitude, bounds.MaximumLongitude);
            if (_cache.TryGetValue(cacheKey, out OsmData? cachedData) && cachedData is not null)
            {
                DataSourceLogMessages.LogCacheHit(_logger, bounds.MinimumLatitude, bounds.MinimumLongitude, bounds.MaximumLatitude, bounds.MaximumLongitude);
                return CloneForCaller(cachedData);
            }

            var data = await FetchCoalescedAsync(bounds, cacheKey, cancellationToken);
            return CloneForCaller(data);
        }

        /// <summary>
        /// Coalesces concurrent fetches for identical <paramref name="cacheKey"/> bounds so that overlapping cache
        /// misses share a single outbound Overpass request instead of each issuing their own. The winning caller's
        /// <paramref name="cancellationToken"/> governs the shared request; a losing caller that cancels only stops
        /// waiting on its own await, it does not cancel the in-flight fetch other callers are still waiting on.
        /// </summary>
        private async Task<OsmData> FetchCoalescedAsync(OsmCoordinateBounds bounds, CacheKey cacheKey, CancellationToken cancellationToken)
        {
            var inFlight = InFlightFetchesByCache.GetValue(_cache, static _ => new ConcurrentDictionary<CacheKey, Lazy<Task<OsmData>>>());

            var lazyFetch = inFlight.GetOrAdd(
                cacheKey,
                _ => new Lazy<Task<OsmData>>(() => FetchAndCacheAsync(bounds, cacheKey, cancellationToken), LazyThreadSafetyMode.ExecutionAndPublication));

            try
            {
                return await lazyFetch.Value;
            }
            finally
            {
                inFlight.TryRemove(new KeyValuePair<CacheKey, Lazy<Task<OsmData>>>(cacheKey, lazyFetch));
            }
        }

        /// <summary>
        /// Runs the fetch-and-parse sequence, retrying up to <see cref="_maxRetryAttempts"/> additional times on a
        /// transient failure (see <see cref="IsTransientFailure"/>) with a short fixed delay between attempts. Because
        /// this runs inside the <see cref="Lazy{T}"/> that <see cref="FetchCoalescedAsync"/> coalesces concurrent
        /// callers onto, a single retry sequence is shared by all of them rather than each paying for its own.
        /// </summary>
        private async Task<OsmData> FetchAndCacheAsync(OsmCoordinateBounds bounds, CacheKey cacheKey, CancellationToken cancellationToken)
        {
            var query = BuildQuery(bounds);
            var attempt = 0;

            while (true)
            {
                try
                {
                    var data = await FetchOnceAsync(query, bounds, cancellationToken);

                    var weight = data.Nodes.Count + data.Ways.Count + data.Relations.Count;
                    _cache.Set(cacheKey, data, new MemoryCacheEntryOptions
                    {
                        Size = weight,
                        AbsoluteExpirationRelativeToNow = _cacheDuration
                    });

                    return data;
                }
                catch (Exception ex) when (attempt < _maxRetryAttempts && IsTransientFailure(ex))
                {
                    attempt++;
                    DataSourceLogMessages.LogRetrying(_logger, attempt, _maxRetryAttempts + 1, ex.Message);
                    await Task.Delay(RetryDelayOverride ?? DefaultRetryDelay, cancellationToken);
                }
            }
        }

        private async Task<OsmData> FetchOnceAsync(string query, OsmCoordinateBounds bounds, CancellationToken cancellationToken)
        {
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

            // Parsing the response body as JSON is potentially expensive for a large bounding box, so the remark
            // check and the deserialize both reuse a single parse: when the default deserializer is in use, the
            // document parsed here is handed straight to it; only a caller-supplied, non-default deserializer
            // (which only exposes a string-based API) falls back to letting it parse the body itself.
            var data = _deserializer is OsmJsonDeserializer concreteDeserializer
                ? ParseResponse(body, concreteDeserializer)
                : await ParseResponseAsync(body, cancellationToken);

            DataSourceLogMessages.LogFetchResult(_logger, data.Nodes.Count, data.Ways.Count, data.Relations.Count);

            return data;
        }

        /// <summary>
        /// Identifies failures worth retrying: HTTP-level responses Overpass returns under load or during a
        /// deployment (429, 502, 503, 504), and <see cref="OverpassQueryFailedException"/> (Overpass's HTTP-200
        /// server-side timeout/memory-ceiling failure). A malformed/unparseable response body is deliberately not
        /// retried, since the same failure would just recur.
        /// </summary>
        private static bool IsTransientFailure(Exception ex) => ex switch
        {
            HttpRequestException httpRequestException => httpRequestException.StatusCode is HttpStatusCode.TooManyRequests
                or HttpStatusCode.BadGateway
                or HttpStatusCode.ServiceUnavailable
                or HttpStatusCode.GatewayTimeout,
            OverpassQueryFailedException => true,
            _ => false
        };

        private OsmData ParseResponse(string body, OsmJsonDeserializer deserializer)
        {
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(body);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Overpass returned a response that could not be parsed as JSON.", ex);
            }

            using (document)
            {
                var remark = TryGetRemark(document.RootElement);
                if (remark is not null)
                {
                    DataSourceLogMessages.LogRemarkDetected(_logger, remark);
                    throw new OverpassQueryFailedException(remark);
                }

                return deserializer.Deserialize(document);
            }
        }

        private async Task<OsmData> ParseResponseAsync(string body, CancellationToken cancellationToken)
        {
            var remark = TryGetRemark(body);
            if (remark is not null)
            {
                DataSourceLogMessages.LogRemarkDetected(_logger, remark);
                throw new OverpassQueryFailedException(remark);
            }

            return await _deserializer.DeserializeAsync(body, cancellationToken);
        }

        /// <summary>
        /// Creates a shallow copy of <paramref name="data"/> so the instance held by the cache is never handed
        /// out directly, preventing a caller's mutation of its own result from corrupting the cached entry.
        /// </summary>
        private static OsmData CloneForCaller(OsmData data) =>
            new(data.Header, data.Bounds, data.Nodes, data.Ways, data.Relations);

        /// <summary>
        /// Cache and in-flight-fetch key derived from a bounding box's four coordinate values, using exact equality.
        /// </summary>
        private readonly record struct CacheKey(double MinimumLatitude, double MinimumLongitude, double MaximumLatitude, double MaximumLongitude);

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
            try
            {
                using var document = JsonDocument.Parse(body);
                return TryGetRemark(document.RootElement);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Overpass returned a response that could not be parsed as JSON.", ex);
            }
        }

        private static string? TryGetRemark(JsonElement root) =>
            root.TryGetProperty("remark", out var remarkElement) && remarkElement.ValueKind == JsonValueKind.String
                ? remarkElement.GetString()
                : null;

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
