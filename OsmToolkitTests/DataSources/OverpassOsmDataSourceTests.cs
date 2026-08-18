using OsmToolkit.DataSources;
using System.Net;
using System.Text;

namespace OsmToolkit.Tests.DataSources
{
    internal class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseContent;

        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        public FakeHttpMessageHandler(HttpStatusCode statusCode, string responseContent)
        {
            _statusCode = statusCode;
            _responseContent = responseContent;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

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

        private static readonly OsmCoordinateBounds Bounds = new(10.0, 20.0, 11.0, 21.0);

        [TestCleanup]
        public void Cleanup()
        {
            OverpassOsmDataSource.DefaultHttpClientOverride = null;
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
    }
}
