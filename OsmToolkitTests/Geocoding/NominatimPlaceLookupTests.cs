using Microsoft.Extensions.Caching.Memory;
using OsmToolkit.Geocoding;
using OsmToolkit.Tests.DataSources;
using System.Net;

namespace OsmToolkit.Tests.Geocoding
{
    [TestClass]
    public class NominatimPlaceLookupTests
    {
        private const string ValidNominatimJson = """
            [
              {
                "place_id": 123,
                "licence": "Data © OpenStreetMap contributors, ODbL 1.0",
                "osm_type": "relation",
                "osm_id": 406071,
                "boundingbox": ["59.0", "59.3", "10.6", "11.0"],
                "lat": "59.15",
                "lon": "10.85",
                "display_name": "Fredrikstad, Ostfold, Norway",
                "class": "boundary",
                "type": "administrative",
                "importance": 0.7
              }
            ]
            """;

        [TestInitialize]
        public void Initialize()
        {
            // A fresh cache per test keeps tests isolated from each other, since the constructor
            // falls back to this override instead of the process-wide shared default cache.
            NominatimPlaceLookup.DefaultCacheOverride = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = NominatimPlaceLookup.DefaultCacheSizeLimit
            });
            // A no-op delay keeps tests fast; the one test that cares about gating installs its own override.
            NominatimPlaceLookup.DefaultRateGateOverride = new RateGate(TimeSpan.FromSeconds(1))
            {
                Delay = (_, _) => Task.CompletedTask
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            NominatimPlaceLookup.DefaultHttpClientOverride = null;
            NominatimPlaceLookup.DefaultCacheOverride = null;
            NominatimPlaceLookup.DefaultRateGateOverride = null;
        }

        [TestMethod]
        public async Task FindAsync_WhenRequestSucceeds_ReturnsParsedResult()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidNominatimJson);
            var httpClient = new HttpClient(handler);
            var sut = new NominatimPlaceLookup(httpClient);

            // Act
            var result = await sut.FindAsync("Fredrikstad");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Fredrikstad, Ostfold, Norway", result.DisplayName);
            Assert.AreEqual(59.15, result.Latitude);
            Assert.AreEqual(10.85, result.Longitude);
        }

        [TestMethod]
        public async Task FindAsync_ParsesNominatimBoundingBoxOrder_ReturnsCorrectlyOrderedBounds()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidNominatimJson);
            var httpClient = new HttpClient(handler);
            var sut = new NominatimPlaceLookup(httpClient);

            // Act
            var result = await sut.FindAsync("Fredrikstad");

            // Assert
            // Nominatim orders its bounding box [minLat, maxLat, minLon, maxLon] — different from
            // OsmCoordinateBounds' (minLat, minLon, maxLat, maxLon) constructor order.
            Assert.AreEqual(59.0, result.Bounds.MinimumLatitude);
            Assert.AreEqual(59.3, result.Bounds.MaximumLatitude);
            Assert.AreEqual(10.6, result.Bounds.MinimumLongitude);
            Assert.AreEqual(11.0, result.Bounds.MaximumLongitude);
        }

        [TestMethod]
        public async Task FindAsync_WithCallerSuppliedHttpClient_SetsIdentificationUserAgentHeader()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidNominatimJson);
            var httpClient = new HttpClient(handler);
            var sut = new NominatimPlaceLookup(httpClient);

            // Act
            await sut.FindAsync("Fredrikstad");

            // Assert
            Assert.IsNotNull(handler.LastRequest);
            var userAgent = handler.LastRequest!.Headers.UserAgent.ToString();
            StringAssert.Contains(userAgent, "OsmToolkit");
            StringAssert.Contains(userAgent, "github.com/Skjobbe/osmtoolkit");
        }

        [TestMethod]
        public async Task FindAsync_WithDefaultInternalHttpClient_SetsIdentificationUserAgentHeader()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidNominatimJson);
            NominatimPlaceLookup.DefaultHttpClientOverride = new HttpClient(handler);
            var sut = new NominatimPlaceLookup();

            // Act
            await sut.FindAsync("Fredrikstad");

            // Assert
            Assert.IsNotNull(handler.LastRequest);
            var userAgent = handler.LastRequest!.Headers.UserAgent.ToString();
            StringAssert.Contains(userAgent, "OsmToolkit");
            StringAssert.Contains(userAgent, "github.com/Skjobbe/osmtoolkit");
        }

        [TestMethod]
        public async Task FindAsync_WhenResponseIsNonSuccess_ThrowsHttpRequestExceptionWithStatusCode()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.TooManyRequests, string.Empty);
            var httpClient = new HttpClient(handler);
            var sut = new NominatimPlaceLookup(httpClient);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => sut.FindAsync("Fredrikstad"));

            // Assert
            Assert.AreEqual(HttpStatusCode.TooManyRequests, exception.StatusCode);
        }

        [TestMethod]
        public async Task FindAsync_WhenResponseHasNoResults_ThrowsPlaceNotFoundException()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "[]");
            var httpClient = new HttpClient(handler);
            var sut = new NominatimPlaceLookup(httpClient);

            // Act
            var exception = await Assert.ThrowsExceptionAsync<PlaceNotFoundException>(
                () => sut.FindAsync("Nonexistentplacexyz"));

            // Assert
            Assert.AreEqual("Nonexistentplacexyz", exception.PlaceName);
        }

        [TestMethod]
        public async Task FindAsync_WhenResponseBodyIsNotJson_ThrowsInvalidOperationException()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "<html>rate limited</html>");
            var httpClient = new HttpClient(handler);
            var sut = new NominatimPlaceLookup(httpClient);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => sut.FindAsync("Fredrikstad"));
        }

        [TestMethod]
        public async Task FindAsync_WhenPlaceNameIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidNominatimJson);
            var httpClient = new HttpClient(handler);
            var sut = new NominatimPlaceLookup(httpClient);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => sut.FindAsync(string.Empty));
        }

        [TestMethod]
        public async Task FindAsync_WhenCalledTwiceForSamePlace_SecondCallIsServedFromCache()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidNominatimJson);
            var httpClient = new HttpClient(handler);
            var sut = new NominatimPlaceLookup(httpClient);

            // Act
            var first = await sut.FindAsync("Fredrikstad");
            var second = await sut.FindAsync("Fredrikstad");

            // Assert
            Assert.AreEqual(1, handler.InvocationCount);
            Assert.AreEqual(first.DisplayName, second.DisplayName);
        }

        [TestMethod]
        public async Task FindAsync_WhenCacheEntryExpires_RefetchesFromNetwork()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidNominatimJson);
            var httpClient = new HttpClient(handler);
            var sut = new NominatimPlaceLookup(httpClient, cacheDuration: TimeSpan.FromMilliseconds(50));

            // Act
            await sut.FindAsync("Fredrikstad");
            await Task.Delay(200);
            await sut.FindAsync("Fredrikstad");

            // Assert
            Assert.AreEqual(2, handler.InvocationCount);
        }

        [TestMethod]
        public async Task FindAsync_WhenEntryExceedsCacheSizeLimit_IsNeverRetainedAndAlwaysRefetches()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidNominatimJson);
            var httpClient = new HttpClient(handler);
            var tinyCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 0 });
            var sut = new NominatimPlaceLookup(httpClient, cache: tinyCache);

            // Act
            await sut.FindAsync("Fredrikstad");
            await sut.FindAsync("Fredrikstad");

            // Assert
            Assert.AreEqual(2, handler.InvocationCount);
        }

        [TestMethod]
        public async Task FindAsync_WhenCalledTwiceForDifferentPlacesBackToBack_SecondCallWaitsAtLeastMinimumInterval()
        {
            // Arrange
            var recordedDelays = new List<TimeSpan>();
            NominatimPlaceLookup.DefaultRateGateOverride = new RateGate(TimeSpan.FromSeconds(1))
            {
                Delay = (duration, _) => { recordedDelays.Add(duration); return Task.CompletedTask; }
            };
            var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ValidNominatimJson);
            var httpClient = new HttpClient(handler);
            var sut = new NominatimPlaceLookup(httpClient);

            // Act
            await sut.FindAsync("Fredrikstad");
            await sut.FindAsync("Oslo");

            // Assert
            Assert.AreEqual(1, recordedDelays.Count);
            Assert.IsTrue(recordedDelays[0] >= TimeSpan.FromMilliseconds(900));
        }
    }
}
