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
    public class SearchByTagsInAreaHandlerTests
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

        // node 1/2 stand alone; node 3/4 are only referenced through way 10, which is in turn referenced
        // through relation 20 - giving each entity type (node, way, relation) its own tagged match, plus a
        // way/relation whose coordinates must be resolved by averaging their referenced nodes.
        private const string ValidOverpassJson = """
            {
              "version": 0.6,
              "generator": "Overpass API",
              "elements": [
                { "type": "node", "id": 1, "lat": 59.20, "lon": 10.95, "tags": { "amenity": "cafe", "name": "Cafe A" } },
                { "type": "node", "id": 2, "lat": 59.21, "lon": 10.96, "tags": { "amenity": "bar" } },
                { "type": "node", "id": 3, "lat": 59.19, "lon": 10.94, "tags": {} },
                { "type": "node", "id": 4, "lat": 59.22, "lon": 10.97, "tags": {} },
                { "type": "way", "id": 10, "nodes": [3, 4], "tags": { "amenity": "cafe", "name": "Cafe Building" } },
                { "type": "relation", "id": 20, "members": [{ "type": "way", "ref": 10, "role": "outer" }], "tags": { "amenity": "cafe", "name": "Cafe Complex" } }
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

        private static SearchByTagsInAreaHandler CreateHandler(string placeJson, string overpassJson)
        {
            var placeLookup = new NominatimPlaceLookup(new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.OK, placeJson)));
            var dataSource = new OverpassOsmDataSource(new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.OK, overpassJson)));
            return new SearchByTagsInAreaHandler(placeLookup, dataSource, new OsmEntityFinder());
        }

        [TestMethod]
        public async Task SearchAsync_WithMatchingKeyAndValue_ReturnsMatchingNodeWithTagsAndCoordinates()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act
            var results = await sut.SearchAsync("Fredrikstad", new Dictionary<string, string?> { ["amenity"] = "cafe" });

            // Assert
            var node = results.Single(r => r.Id == 1);
            Assert.AreEqual("node", node.EntityType);
            Assert.AreEqual("cafe", node.Tags["amenity"]);
            Assert.AreEqual("Cafe A", node.Tags["name"]);
            Assert.AreEqual(59.20, node.Latitude);
            Assert.AreEqual(10.95, node.Longitude);
        }

        [TestMethod]
        public async Task SearchAsync_WithNullTagValue_MatchesAnyValueForKey()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act
            var results = await sut.SearchAsync("Fredrikstad", new Dictionary<string, string?> { ["amenity"] = null });

            // Assert
            // node 1 (cafe), node 2 (bar), way 10 (cafe), relation 20 (cafe) - every entity carrying an
            // "amenity" tag, regardless of its value.
            Assert.AreEqual(4, results.Count);
        }

        [TestMethod]
        public async Task SearchAsync_WithMultipleTagFilters_RequiresAllToMatch()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act
            var results = await sut.SearchAsync("Fredrikstad", new Dictionary<string, string?>
            {
                ["amenity"] = "cafe",
                ["name"] = "Cafe Building",
            });

            // Assert
            var match = results.Single();
            Assert.AreEqual(10, match.Id);
        }

        [TestMethod]
        public async Task SearchAsync_ForWay_ResolvesCoordinatesAsAverageOfReferencedNodes()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act
            var results = await sut.SearchAsync("Fredrikstad", new Dictionary<string, string?> { ["amenity"] = "cafe" });

            // Assert
            var way = results.Single(r => r.Id == 10);
            Assert.AreEqual("way", way.EntityType);
            Assert.AreEqual((59.19 + 59.22) / 2, way.Latitude);
            Assert.AreEqual((10.94 + 10.97) / 2, way.Longitude);
        }

        [TestMethod]
        public async Task SearchAsync_ForRelation_ResolvesCoordinatesFromReferencedWayNodes()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act
            var results = await sut.SearchAsync("Fredrikstad", new Dictionary<string, string?> { ["amenity"] = "cafe" });

            // Assert
            var relation = results.Single(r => r.Id == 20);
            Assert.AreEqual("relation", relation.EntityType);
            Assert.AreEqual((59.19 + 59.22) / 2, relation.Latitude);
            Assert.AreEqual((10.94 + 10.97) / 2, relation.Longitude);
        }

        [TestMethod]
        public async Task SearchAsync_WhenNoTagsMatch_ReturnsEmpty()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act
            var results = await sut.SearchAsync("Fredrikstad", new Dictionary<string, string?> { ["shop"] = "supermarket" });

            // Assert
            Assert.AreEqual(0, results.Count);
        }

        [TestMethod]
        public async Task SearchAsync_WhenPlaceIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => sut.SearchAsync(string.Empty, new Dictionary<string, string?> { ["amenity"] = "cafe" }));
        }

        [TestMethod]
        public async Task SearchAsync_WhenTagsIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var sut = CreateHandler(ValidNominatimJson, ValidOverpassJson);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => sut.SearchAsync("Fredrikstad", new Dictionary<string, string?>()));
        }

        [TestMethod]
        public async Task SearchAsync_WhenPlaceNotFound_PropagatesPlaceNotFoundException()
        {
            // Arrange
            var sut = CreateHandler("[]", ValidOverpassJson);

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<PlaceNotFoundException>(
                () => sut.SearchAsync("Nonexistentplacexyz", new Dictionary<string, string?> { ["amenity"] = "cafe" }));
            Assert.AreEqual("Nonexistentplacexyz", exception.PlaceName);
        }
    }
}
