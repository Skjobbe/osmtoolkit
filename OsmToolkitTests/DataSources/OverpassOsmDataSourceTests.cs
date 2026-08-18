using Microsoft.Extensions.Caching.Memory;
using OsmToolkit.DataSources;
using System.Net;
using System.Text;

namespace OsmToolkit.Tests.DataSources
{
    internal class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseContent;
        private readonly TimeSpan _delay;

        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }
        public int InvocationCount { get; private set; }

        public FakeHttpMessageHandler(HttpStatusCode statusCode, string responseContent, TimeSpan? delay = null)
        {
            _statusCode = statusCode;
            _responseContent = responseContent;
            _delay = delay ?? TimeSpan.Zero;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            InvocationCount++;
            LastRequest = request;
            LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

            if (_delay > TimeSpan.Zero)
                await Task.Delay(_delay, cancellationToken);

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent, Encoding.UTF8, "application/json")
            };
        }
    }

    [TestClass]
    public class OverpassOsmDataSourceTests
    {
        private const string ValidOverpassJson = """
            {
              "version": 0.6,
              "generator": "Overpass API",
              "elements": [
                { "type": "node", "id": 1, "lat": 10.0, "lon": 20.0, "tags": {} }
              ]
            }
            """;

        private static readonly OsmCoordinateBounds Bounds = new(10.0, 20.0, 10.5, 20.5);

        [TestInitialize]
        public void Initialize()
        {
            // A fresh cache per test keeps tests isolated from each other, since the constructor
            // falls back to this override instead of the process-wide shared default cache.
            OverpassOsmDataSource.DefaultCacheOverride = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = OverpassOsmDataSource.DefaultCacheSizeLimit
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            OverpassOsmDataSource.DefaultHttpClientOverride = null;
            OverpassOsmDataSource.DefaultCacheOverride = null;
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WhenRequestSucceeds_ReturnsParsedData()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidOverpassJson);
            var httpClient = new HttpClient(handler);
            var sut = new OverpassOsmDataSource(httpClient);

            // Act
            var result = await sut.GetOsmDataAsync(Bounds);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Nodes.Count);
            Assert.AreEqual(1, result.Nodes[0].Id);
        }

        [TestMethod]
        public async Task GetOsmDataAsync_BuildsQuery_ContainsRecursionAndServerSideLimitClauses()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidOverpassJson);
            var httpClient = new HttpClient(handler);
            var sut = new OverpassOsmDataSource(httpClient, queryTimeoutSeconds: 25, queryMaxSizeBytes: 1_073_741_824L);

            // Act
            await sut.GetOsmDataAsync(Bounds);

            // Assert
            Assert.IsNotNull(handler.LastRequestBody);
            var decodedBody = Uri.UnescapeDataString(handler.LastRequestBody!);
            StringAssert.Contains(decodedBody, "[timeout:25]");
            StringAssert.Contains(decodedBody, "[maxsize:1073741824]");
            StringAssert.Contains(decodedBody, ">;");
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WithCallerSuppliedHttpClient_SetsIdentificationUserAgentHeader()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidOverpassJson);
            var httpClient = new HttpClient(handler);
            var sut = new OverpassOsmDataSource(httpClient);

            // Act
            await sut.GetOsmDataAsync(Bounds);

            // Assert
            Assert.IsNotNull(handler.LastRequest);
            var userAgent = handler.LastRequest!.Headers.UserAgent.ToString();
            StringAssert.Contains(userAgent, "OsmToolkit");
            StringAssert.Contains(userAgent, "github.com/Skjobbe/osmtoolkit");
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WithDefaultInternalHttpClient_SetsIdentificationUserAgentHeader()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidOverpassJson);
            OverpassOsmDataSource.DefaultHttpClientOverride = new HttpClient(handler);
            var sut = new OverpassOsmDataSource();

            // Act
            await sut.GetOsmDataAsync(Bounds);

            // Assert
            Assert.IsNotNull(handler.LastRequest);
            var userAgent = handler.LastRequest!.Headers.UserAgent.ToString();
            StringAssert.Contains(userAgent, "OsmToolkit");
            StringAssert.Contains(userAgent, "github.com/Skjobbe/osmtoolkit");
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WhenResponseIsNonSuccess_ThrowsHttpRequestExceptionWithStatusCode()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.TooManyRequests, string.Empty);
            var httpClient = new HttpClient(handler);
            var sut = new OverpassOsmDataSource(httpClient);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => sut.GetOsmDataAsync(Bounds));

            // Assert
            Assert.AreEqual(HttpStatusCode.TooManyRequests, exception.StatusCode);
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WhenResponseHasRemarkField_ThrowsInvalidOperationExceptionWithRemarkText()
        {
            // Arrange
            const string remarkText = "runtime error: Query timed out in \"query\" at line 5 after 26 seconds.";
            var escapedRemarkText = remarkText.Replace("\"", "\\\"");
            var remarkJson = $$"""
                {
                  "version": 0.6,
                  "generator": "Overpass API",
                  "elements": [],
                  "remark": "{{escapedRemarkText}}"
                }
                """;
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, remarkJson);
            var httpClient = new HttpClient(handler);
            var sut = new OverpassOsmDataSource(httpClient);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => sut.GetOsmDataAsync(Bounds));

            // Assert
            Assert.AreEqual(remarkText, exception.Message);
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WhenResponseHasNoRemarkField_ReturnsParsedDataNormally()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidOverpassJson);
            var httpClient = new HttpClient(handler);
            var sut = new OverpassOsmDataSource(httpClient);

            // Act
            var result = await sut.GetOsmDataAsync(Bounds);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Nodes.Count);
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WhenBoundsIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidOverpassJson);
            var httpClient = new HttpClient(handler);
            var sut = new OverpassOsmDataSource(httpClient);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                () => sut.GetOsmDataAsync(null!));
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WhenBoundsExceedDefaultAreaCeiling_ThrowsWithoutInvokingHttpHandler()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidOverpassJson);
            var httpClient = new HttpClient(handler);
            var sut = new OverpassOsmDataSource(httpClient);
            var oversizedBounds = new OsmCoordinateBounds(0.0, 0.0, 10.0, 10.0);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
                () => sut.GetOsmDataAsync(oversizedBounds));
            Assert.IsNull(handler.LastRequest);
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WhenBoundsUnderDefaultAreaCeiling_ProceedsNormally()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidOverpassJson);
            var httpClient = new HttpClient(handler);
            var sut = new OverpassOsmDataSource(httpClient);

            // Act
            var result = await sut.GetOsmDataAsync(Bounds);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(handler.LastRequest);
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WithCustomAreaCeiling_RejectsBoundsWithinDefaultButOverCustomCeiling()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidOverpassJson);
            var httpClient = new HttpClient(handler);
            var sut = new OverpassOsmDataSource(httpClient, maxAreaSquareKilometers: 100);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
                () => sut.GetOsmDataAsync(Bounds));
            Assert.IsNull(handler.LastRequest);
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WhenCalledTwiceForSameBounds_SecondCallIsServedFromCache()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidOverpassJson);
            var httpClient = new HttpClient(handler);
            var sut = new OverpassOsmDataSource(httpClient);

            // Act
            var first = await sut.GetOsmDataAsync(Bounds);
            var second = await sut.GetOsmDataAsync(Bounds);

            // Assert
            Assert.AreEqual(1, handler.InvocationCount);
            Assert.AreEqual(first.Nodes.Count, second.Nodes.Count);
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WhenCalledConcurrentlyForSameBoundsDuringCacheMiss_CoalescesIntoSingleRequest()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidOverpassJson, delay: TimeSpan.FromMilliseconds(200));
            var httpClient = new HttpClient(handler);
            var sut = new OverpassOsmDataSource(httpClient);

            // Act
            var firstTask = sut.GetOsmDataAsync(Bounds);
            var secondTask = sut.GetOsmDataAsync(Bounds);
            var results = await Task.WhenAll(firstTask, secondTask);

            // Assert
            Assert.AreEqual(1, handler.InvocationCount);
            Assert.AreEqual(1, results[0].Nodes.Count);
            Assert.AreEqual(1, results[1].Nodes.Count);
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WhenCacheEntryExpires_RefetchesFromNetwork()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidOverpassJson);
            var httpClient = new HttpClient(handler);
            var sut = new OverpassOsmDataSource(httpClient, cacheDuration: TimeSpan.FromMilliseconds(50));

            // Act
            await sut.GetOsmDataAsync(Bounds);
            await Task.Delay(200);
            await sut.GetOsmDataAsync(Bounds);

            // Assert
            Assert.AreEqual(2, handler.InvocationCount);
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WhenEntryExceedsCacheSizeLimit_IsNeverRetainedAndAlwaysRefetches()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidOverpassJson);
            var httpClient = new HttpClient(handler);
            var tinyCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 0 });
            var sut = new OverpassOsmDataSource(httpClient, cache: tinyCache);

            // Act
            await sut.GetOsmDataAsync(Bounds);
            await sut.GetOsmDataAsync(Bounds);

            // Assert
            Assert.AreEqual(2, handler.InvocationCount);
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WhenCallerMutatesReturnedResult_CachedEntryIsUnaffected()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidOverpassJson);
            var httpClient = new HttpClient(handler);
            var sut = new OverpassOsmDataSource(httpClient);

            // Act
            var first = await sut.GetOsmDataAsync(Bounds);
            first.Nodes.Add(new Node(999, 0, 0));
            first.SortOsmData();
            var second = await sut.GetOsmDataAsync(Bounds);

            // Assert
            Assert.AreEqual(1, second.Nodes.Count);
            Assert.AreEqual(1, handler.InvocationCount);
        }

        [TestMethod]
        public async Task GetOsmDataAsync_WhenResponseBodyIsNotJson_ThrowsInvalidOperationException()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "<html>rate limited</html>");
            var httpClient = new HttpClient(handler);
            var sut = new OverpassOsmDataSource(httpClient);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => sut.GetOsmDataAsync(Bounds));
        }
    }
}
