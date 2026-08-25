using Microsoft.Extensions.Caching.Memory;
using OsmToolkit.DataSources;
using OsmToolkit.Finders;
using OsmToolkit.Geocoding;
using OsmToolkit.Mcp.Tools;
using OsmToolkit.Tests.DataSources;
using System.Net;
using System.Text;

namespace OsmToolkit.Tests.Mcp
{
    /// <summary>
    /// Fakes Nominatim's HTTP boundary by matching each request's query string against a place name,
    /// unlike <see cref="FakeHttpMessageHandler"/> which always returns the same fixed response - needed
    /// here because a single route resolves two different place names (origin and destination) against
    /// the same <see cref="HttpClient"/>.
    /// </summary>
    internal sealed class PlaceLookupFakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly IReadOnlyDictionary<string, string> _responseJsonByPlaceName;

        public PlaceLookupFakeHttpMessageHandler(IReadOnlyDictionary<string, string> responseJsonByPlaceName)
            => _responseJsonByPlaceName = responseJsonByPlaceName;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var query = request.RequestUri!.Query;
            var placeName = _responseJsonByPlaceName.Keys.First(name => query.Contains(Uri.EscapeDataString(name)));

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseJsonByPlaceName[placeName], Encoding.UTF8, "application/json")
            });
        }
    }

    [TestClass]
    public class RouteBetweenPointsHandlerTests
    {
        private const string OriginNominatimJson = """
            [
              {
                "place_id": 1,
                "licence": "Data © OpenStreetMap contributors, ODbL 1.0",
                "osm_type": "node",
                "osm_id": 1,
                "boundingbox": ["59.19", "59.21", "10.89", "10.91"],
                "lat": "59.20",
                "lon": "10.90",
                "display_name": "Fredrikstad Origin, Norway",
                "class": "place",
                "type": "town",
                "importance": 0.7
              }
            ]
            """;

        private const string DestinationNominatimJson = """
            [
              {
                "place_id": 2,
                "licence": "Data © OpenStreetMap contributors, ODbL 1.0",
                "osm_type": "node",
                "osm_id": 2,
                "boundingbox": ["59.21", "59.23", "10.94", "10.96"],
                "lat": "59.22",
                "lon": "10.95",
                "display_name": "Fredrikstad Destination, Norway",
                "class": "place",
                "type": "suburb",
                "importance": 0.6
              }
            ]
            """;

        private const string FarOriginNominatimJson = """
            [
              {
                "place_id": 3,
                "licence": "Data © OpenStreetMap contributors, ODbL 1.0",
                "osm_type": "node",
                "osm_id": 3,
                "boundingbox": ["-0.1", "0.1", "-0.1", "0.1"],
                "lat": "0.0",
                "lon": "0.0",
                "display_name": "Null Island",
                "class": "place",
                "type": "town",
                "importance": 0.5
              }
            ]
            """;

        private const string FarDestinationNominatimJson = """
            [
              {
                "place_id": 4,
                "licence": "Data © OpenStreetMap contributors, ODbL 1.0",
                "osm_type": "node",
                "osm_id": 4,
                "boundingbox": ["59.9", "60.1", "59.9", "60.1"],
                "lat": "60.0",
                "lon": "60.0",
                "display_name": "Far Away Place",
                "class": "place",
                "type": "town",
                "importance": 0.5
              }
            ]
            """;

        // Nodes exactly on the origin/destination centroids, connected through a middle node - lets the
        // happy-path test assert waypoint order, not just that a route exists.
        private const string ConnectedOverpassJson = """
            {
              "version": 0.6,
              "generator": "Overpass API",
              "elements": [
                { "type": "node", "id": 1, "lat": 59.20, "lon": 10.90, "tags": {} },
                { "type": "node", "id": 3, "lat": 59.21, "lon": 10.925, "tags": {} },
                { "type": "node", "id": 2, "lat": 59.22, "lon": 10.95, "tags": {} },
                { "type": "way", "id": 10, "nodes": [1, 3, 2], "tags": { "highway": "residential" } }
              ]
            }
            """;

        // Same two endpoints, no way connecting them.
        private const string DisconnectedOverpassJson = """
            {
              "version": 0.6,
              "generator": "Overpass API",
              "elements": [
                { "type": "node", "id": 1, "lat": 59.20, "lon": 10.90, "tags": {} },
                { "type": "node", "id": 2, "lat": 59.22, "lon": 10.95, "tags": {} }
              ]
            }
            """;

        // Origin and destination connected only by a footway - excluded for motorcar, allowed for foot.
        private const string FootwayOnlyOverpassJson = """
            {
              "version": 0.6,
              "generator": "Overpass API",
              "elements": [
                { "type": "node", "id": 1, "lat": 59.20, "lon": 10.90, "tags": {} },
                { "type": "node", "id": 2, "lat": 59.22, "lon": 10.95, "tags": {} },
                { "type": "way", "id": 10, "nodes": [1, 2], "tags": { "highway": "footway" } }
              ]
            }
            """;

        // Origin and destination connected only by a motorway - excluded when avoidMotorway is set.
        private const string MotorwayOnlyOverpassJson = """
            {
              "version": 0.6,
              "generator": "Overpass API",
              "elements": [
                { "type": "node", "id": 1, "lat": 59.20, "lon": 10.90, "tags": {} },
                { "type": "node", "id": 2, "lat": 59.22, "lon": 10.95, "tags": {} },
                { "type": "way", "id": 10, "nodes": [1, 2], "tags": { "highway": "motorway" } }
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

        private static RouteBetweenPointsHandler CreateHandler(string overpassJson, IReadOnlyDictionary<string, string>? placeResponses = null)
        {
            var responses = placeResponses ?? new Dictionary<string, string>
            {
                ["Origin"] = OriginNominatimJson,
                ["Destination"] = DestinationNominatimJson,
            };

            var placeLookup = new NominatimPlaceLookup(new HttpClient(new PlaceLookupFakeHttpMessageHandler(responses)));
            var dataSource = new OverpassOsmDataSource(new HttpClient(new FakeHttpMessageHandler(HttpStatusCode.OK, overpassJson)));
            return new RouteBetweenPointsHandler(placeLookup, dataSource, new OsmEntityFinder());
        }

        [TestMethod]
        public async Task RouteAsync_BetweenConnectedPlaces_ReturnsRouteWithWaypointsInPathOrder()
        {
            // Arrange
            var sut = CreateHandler(ConnectedOverpassJson);

            // Act
            var result = await sut.RouteAsync("Origin", "Destination", "car", avoidMotorway: false);

            // Assert
            Assert.AreEqual("Fredrikstad Origin, Norway", result.OriginDisplayName);
            Assert.AreEqual("Fredrikstad Destination, Norway", result.DestinationDisplayName);
            Assert.AreEqual("car", result.TravelMode);
            Assert.IsFalse(result.AvoidMotorway);
            Assert.IsTrue(result.TotalDistanceMeters > 0);
            Assert.IsNull(result.Description);
            CollectionAssert.AreEqual(
                new[] { (59.20, 10.90), (59.21, 10.925), (59.22, 10.95) },
                result.Waypoints.Select(w => (w.Latitude, w.Longitude)).ToArray());
        }

        [TestMethod]
        public async Task RouteAsync_WhenOriginAndDestinationShareLatitude_DoesNotThrow()
        {
            // Arrange
            // A due-east/west route: both places resolve to the same latitude, which would otherwise
            // collapse BoundsSpanning's latitude span to zero width.
            var sameLatitudeDestinationJson = DestinationNominatimJson.Replace("\"lat\": \"59.22\"", "\"lat\": \"59.20\"");
            var sut = CreateHandler(ConnectedOverpassJson, new Dictionary<string, string>
            {
                ["Origin"] = OriginNominatimJson,
                ["Destination"] = sameLatitudeDestinationJson,
            });

            // Act
            var result = await sut.RouteAsync("Origin", "Destination", "car", avoidMotorway: false);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task RouteAsync_WhenNoRouteExists_ReturnsResultWithDescriptionAndEmptyWaypoints()
        {
            // Arrange
            var sut = CreateHandler(DisconnectedOverpassJson);

            // Act
            var result = await sut.RouteAsync("Origin", "Destination", "car", avoidMotorway: false);

            // Assert
            Assert.AreEqual(0, result.Waypoints.Count);
            Assert.AreEqual(0, result.TotalDistanceMeters);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Description));
        }

        [TestMethod]
        public async Task RouteAsync_WithFootTravelMode_AllowsFootwayOnlyRoute()
        {
            // Arrange
            var sut = CreateHandler(FootwayOnlyOverpassJson);

            // Act
            var result = await sut.RouteAsync("Origin", "Destination", "foot", avoidMotorway: false);

            // Assert
            Assert.AreEqual("foot", result.TravelMode);
            Assert.IsNull(result.Description);
            Assert.AreEqual(2, result.Waypoints.Count);
        }

        [TestMethod]
        public async Task RouteAsync_WithCarTravelMode_ExcludesFootwayOnlyRoute()
        {
            // Arrange
            var sut = CreateHandler(FootwayOnlyOverpassJson);

            // Act
            var result = await sut.RouteAsync("Origin", "Destination", "car", avoidMotorway: false);

            // Assert
            Assert.AreEqual(0, result.Waypoints.Count);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Description));
        }

        [TestMethod]
        public async Task RouteAsync_WithAvoidMotorwayFalse_UsesMotorwayWhenNoAlternative()
        {
            // Arrange
            var sut = CreateHandler(MotorwayOnlyOverpassJson);

            // Act
            var result = await sut.RouteAsync("Origin", "Destination", "car", avoidMotorway: false);

            // Assert
            Assert.IsNull(result.Description);
            Assert.AreEqual(2, result.Waypoints.Count);
        }

        [TestMethod]
        public async Task RouteAsync_WithAvoidMotorwayTrue_ExcludesMotorwayAndFindsNoRoute()
        {
            // Arrange
            var sut = CreateHandler(MotorwayOnlyOverpassJson);

            // Act
            var result = await sut.RouteAsync("Origin", "Destination", "car", avoidMotorway: true);

            // Assert
            Assert.IsTrue(result.AvoidMotorway);
            Assert.AreEqual(0, result.Waypoints.Count);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Description));
        }

        [TestMethod]
        public async Task RouteAsync_WhenOriginIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var sut = CreateHandler(ConnectedOverpassJson);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => sut.RouteAsync(string.Empty, "Destination", "car", avoidMotorway: false));
        }

        [TestMethod]
        public async Task RouteAsync_WhenDestinationIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var sut = CreateHandler(ConnectedOverpassJson);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => sut.RouteAsync("Origin", string.Empty, "car", avoidMotorway: false));
        }

        [TestMethod]
        public async Task RouteAsync_WhenTravelModeIsInvalid_ThrowsArgumentException()
        {
            // Arrange
            var sut = CreateHandler(ConnectedOverpassJson);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => sut.RouteAsync("Origin", "Destination", "spaceship", avoidMotorway: false));
        }

        [TestMethod]
        public async Task RouteAsync_WhenOriginNotFound_PropagatesPlaceNotFoundException()
        {
            // Arrange
            var sut = CreateHandler(ConnectedOverpassJson, new Dictionary<string, string>
            {
                ["Origin"] = "[]",
                ["Destination"] = DestinationNominatimJson,
            });

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<PlaceNotFoundException>(
                () => sut.RouteAsync("Origin", "Destination", "car", avoidMotorway: false));
            Assert.AreEqual("Origin", exception.PlaceName);
        }

        [TestMethod]
        public async Task RouteAsync_WhenPlacesSpanHugeArea_PropagatesAreaGuardrailException()
        {
            // Arrange
            var sut = CreateHandler(ConnectedOverpassJson, new Dictionary<string, string>
            {
                ["Origin"] = FarOriginNominatimJson,
                ["Destination"] = FarDestinationNominatimJson,
            });

            // Act & Assert
            // OverpassOsmDataSource's own 10,000 km² area guardrail (issue #8) should reject this before
            // any Overpass HTTP call is made - the same guardrail as every other data-fetching path, not
            // a new one specific to routing.
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(
                () => sut.RouteAsync("Origin", "Destination", "car", avoidMotorway: false));
        }
    }
}
