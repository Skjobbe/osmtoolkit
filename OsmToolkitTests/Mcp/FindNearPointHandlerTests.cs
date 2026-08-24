using Microsoft.Extensions.Caching.Memory;
using OsmToolkit.DataSources;
using OsmToolkit.Finders;
using OsmToolkit.Geocoding;
using OsmToolkit.Mcp.Tools;
using OsmToolkit.Tests.DataSources;
using System.Net;

namespace OsmToolkit.Tests.Mcp
{
    [TestClass]
    public class FindNearPointHandlerTests
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

        // Distances from the centroid (59.15, 10.85): node 4 (~22m), node 1 (~55m), node 2 (~334m), node 3 (~2226m).
        private const string ValidOverpassJson = """
            {
              "version": 0.6,
              "generator": "Overpass API",
              "elements": [
                { "type": "node", "id": 1, "lat": 59.1505, "lon": 10.85, "tags": { "amenity": "cafe", "name": "Near Cafe" } },
                { "type": "node", "id": 2, "lat": 59.153, "lon": 10.85, "tags": { "amenity": "bar" } },
                { "type": "node", "id": 3, "lat": 59.17, "lon": 10.85, "tags": { "amenity": "cafe", "name": "Far Cafe" } },
                { "type": "node", "id": 4, "lat": 59.1502, "lon": 10.8503, "tags": {} }
              ]
            }
            """;

        [TestInitialize]
        public void Initialize()
        {
            // Fresh caches per test keep tests isolated from each other, since both constructors fall back
            // to these overrides instead of their process-wide shared default caches.
            NominatimPlaceLookup.DefaultCacheOverride = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = NominatimPlaceLookup.DefaultCacheSizeLimit
            });
            NominatimPlaceLookup.DefaultRateGateOverride = new RateGate(TimeSpan.FromSeconds(1))
            {
                Delay = (_, _) => Task.CompletedTask
            };
            OverpassOsmDataSource.DefaultCacheOverride = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = OverpassOsmDataSource.DefaultCacheSizeLimit
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            NominatimPlaceLookup.DefaultHttpClientOverride = null;
            NominatimPlaceLookup.DefaultCacheOverride = null;
            NominatimPlaceLookup.DefaultRateGateOverride = null;
            OverpassOsmDataSource.DefaultHttpClientOverride = null;
            OverpassOsmDataSource.DefaultCacheOverride = null;
        }

        private static FindNearPointHandler CreateHandler(string placeJson, string overpassJson)
        {
            var placeLookup = new NominatimPlaceLookup(new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.OK, placeJson)));
            var dataSource = new OverpassOsmDataSource(new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.OK, overpassJson)));
            var finder = new OsmEntityFinder();
            return new FindNearPointHandler(placeLookup, dataSource, finder, finder);
        }

        [TestMethod]
        public async Task FindAsync_WithinRadius_ReturnsNodesOrderedByDistanceExcludingFartherNodes()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act
            var results = await sut.FindAsync("Fredrikstad", radiusMeters: 1000, tags: null, limit: 10);

            // Assert
            Assert.AreEqual(3, results.Count);
            CollectionAssert.AreEqual(new long[] { 4, 1, 2 }, results.Select(r => r.Id).ToArray());
            Assert.IsTrue(results[0].DistanceMeters < results[1].DistanceMeters);
            Assert.IsTrue(results[1].DistanceMeters < results[2].DistanceMeters);
        }

        [TestMethod]
        public async Task FindAsync_WithTagFilter_ReturnsOnlyMatchingNodesWithinRadius()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act
            var results = await sut.FindAsync("Fredrikstad", radiusMeters: 1000, tags: new Dictionary<string, string> { ["amenity"] = "cafe" }, limit: 10);

            // Assert
            // Node 1 is a cafe within radius; node 3 is a cafe but outside the 1000m radius; node 2/4 aren't cafes.
            var match = results.Single();
            Assert.AreEqual(1, match.Id);
            Assert.AreEqual("cafe", match.Tags["amenity"]);
        }

        [TestMethod]
        public async Task FindAsync_WithLimitOne_ReturnsOnlyTheNearestNode()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act
            var results = await sut.FindAsync("Fredrikstad", radiusMeters: 1000, tags: null, limit: 1);

            // Assert
            var match = results.Single();
            Assert.AreEqual(4, match.Id);
        }

        [TestMethod]
        public async Task FindAsync_WithLimitOneAndTagFilter_ReturnsNearestMatchingNode()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act
            var results = await sut.FindAsync("Fredrikstad", radiusMeters: 3000, tags: new Dictionary<string, string> { ["amenity"] = "cafe" }, limit: 1);

            // Assert
            // Both node 1 and node 3 are cafes within a 3000m radius; node 1 is nearer.
            var match = results.Single();
            Assert.AreEqual(1, match.Id);
        }

        [TestMethod]
        public async Task FindAsync_WithSmallRadius_ExcludesFartherNodes()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act
            var results = await sut.FindAsync("Fredrikstad", radiusMeters: 100, tags: null, limit: 10);

            // Assert
            CollectionAssert.AreEqual(new long[] { 4, 1 }, results.Select(r => r.Id).ToArray());
        }

        [TestMethod]
        public async Task FindAsync_WhenNoNodesWithinRadius_ReturnsEmpty()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act
            var results = await sut.FindAsync("Fredrikstad", radiusMeters: 5, tags: null, limit: 10);

            // Assert
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public async Task FindAsync_WhenPlaceIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => sut.FindAsync(string.Empty, radiusMeters: 1000, tags: null, limit: 10));
        }

        [TestMethod]
        public async Task FindAsync_WhenRadiusIsZero_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
                () => sut.FindAsync("Fredrikstad", radiusMeters: 0, tags: null, limit: 10));
        }

        [TestMethod]
        public async Task FindAsync_WhenLimitIsZero_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
                () => sut.FindAsync("Fredrikstad", radiusMeters: 1000, tags: null, limit: 0));
        }

        [TestMethod]
        public async Task FindAsync_WhenPlaceNotFound_PropagatesPlaceNotFoundException()
        {
            // Arrange
            var sut = CreateHandler("[]", ValidOverpassJson);

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<PlaceNotFoundException>(
                () => sut.FindAsync("Nonexistentplacexyz", radiusMeters: 1000, tags: null, limit: 10));
            Assert.AreEqual("Nonexistentplacexyz", exception.PlaceName);
        }
    }
}
