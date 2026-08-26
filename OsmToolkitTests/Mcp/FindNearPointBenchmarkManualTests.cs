using OsmToolkit.DataSources;
using OsmToolkit.Finders;
using OsmToolkit.Finders.Spatial;
using OsmToolkit.Geocoding;
using OsmToolkit.Mcp.Tools;
using System.Diagnostics;
using System.Linq;

namespace OsmToolkit.Tests.Mcp
{
    /// <summary>
    /// End-to-end benchmark of a full <c>find_near_point</c> MCP call, extending
    /// <see cref="Finders.GridIndexBenchmarkManualTests"/>'s pattern (same <c>TestCategory</c>, same real
    /// Fredrikstad Overpass extract technique) from the finder layer alone to the whole MCP-driven path:
    /// geocoding, Overpass fetch, grid-index build, Way/Relation gathering, and finder execution, each
    /// reported as a separate measurement. Exists to decide #18 and #23 on real numbers - does either cost
    /// show up as significant next to geocoding/Overpass network latency in an actual find_near_point call,
    /// or is it noise, the same question ADR-08 already asked once about the grid index itself.
    ///
    /// Doesn't call <see cref="FindNearPointHandler.FindAsync"/> for the timed steps: <see cref="NominatimPlaceLookup"/>
    /// and <see cref="OverpassOsmDataSource"/> both cache their results, so a second call through the handler
    /// for the same place/bounds would be a near-instant cache hit rather than the real network cost this
    /// benchmark needs to measure. Instead it replicates FindAsync's exact call sequence (see
    /// OsmToolkit.Mcp/Tools/FindNearPointHandler.cs) directly, with a Stopwatch around each step, then calls
    /// the handler once more afterward - reusing the same, now-warmed instances, so no extra network calls -
    /// as a correctness check that the manual sequence matches what find_near_point actually returns.
    ///
    /// Excluded from CI via the TestCategory filter in .github/workflows/ci.yml, since it depends on network
    /// access and third-party services' (Nominatim's, Overpass's) availability and rate limits.
    /// Run it manually with: dotnet test --filter "TestCategory=ManualIntegration"
    /// </summary>
    [TestClass]
    [TestCategory("ManualIntegration")]
    public class FindNearPointBenchmarkManualTests
    {
        // Gamlebyen resolves inside the same central-Fredrikstad extent GridIndexBenchmarkManualTests
        // measures (59.19-59.23 lat, 10.92-10.99 lon). Geocoding "Fredrikstad, Norway" itself resolves to
        // the municipality's dense downtown centroid instead, whose bbox overlaps a large administrative
        // boundary relation - the recursive ">;" clause in BuildQuery then pulls in that relation's full
        // member-way node set regardless of the small query radius, which reliably tripped Overpass's
        // reverse-proxy timeout (~15s) even though Overpass's own [timeout:25] budget was never reached.
        private const string Place = "Gamlebyen, Fredrikstad";
        private const double RadiusMeters = 700d;
        private const int QueryPointCount = 25;

        [TestMethod]
        public async Task FindNearPoint_EndToEnd_ReportsPerStepTimingsForDeciding18And23()
        {
            // Arrange
            var placeLookup = new NominatimPlaceLookup();
            var dataSource = new OverpassOsmDataSource();
            var finder = new OsmEntityFinder();

            // Act: geocoding lookup
            var geocodeStopwatch = Stopwatch.StartNew();
            var location = await placeLookup.FindAsync(Place);
            geocodeStopwatch.Stop();

            // Act: Overpass fetch, over the same bounds a real find_near_point call would compute
            var bounds = BoundsFromRadius(location.Latitude, location.Longitude, RadiusMeters);
            var overpassStopwatch = Stopwatch.StartNew();
            var data = await dataSource.GetOsmDataAsync(bounds);
            overpassStopwatch.Stop();
            TestContext.WriteLine($"Fetched {data.Nodes.Count} nodes, {data.Ways.Count} ways, {data.Relations.Count} relations from the real Overpass API, centered on '{Place}'.");
            Assert.IsTrue(data.Nodes.Count >= QueryPointCount,
                $"Expected at least {QueryPointCount} nodes to sample query points from - got {data.Nodes.Count}.");

            // Act: finder execution for the actual call's single query point (the resolved location), on data
            // fresh from the fetch above - the literal finder-execution cost of one real find_near_point call,
            // including the one-time grid-index build it pays as its first-ever query against this OsmData.
            var finderExecutionData = new OsmData(data.Header, data.Bounds, data.Nodes, data.Ways, data.Relations);
            var finderExecutionStopwatch = Stopwatch.StartNew();
            var withinRadiusAtLocation = finder.FindNearByRadius(finderExecutionData, location.Latitude, location.Longitude, RadiusMeters);
            var nearestAtLocation = finder.FindNearbyNodes(withinRadiusAtLocation, location.Latitude, location.Longitude, 10);
            finderExecutionStopwatch.Stop();

            // Act: grid-index build time and Way/Relation gathering time, isolated the same way
            // GridIndexBenchmarkManualTests isolates them - separate OsmData instances per timed comparison,
            // since the grid index is cached per-OsmData-instance identity (see OsmEntityFinder.GetGridIndex,
            // the ConditionalWeakTable at the center of #18).
            int step = data.Nodes.Count / QueryPointCount;
            var queryPoints = Enumerable.Range(0, QueryPointCount).Select(i => data.Nodes[i * step]).ToList();

            var buildData = new OsmData(data.Header, data.Bounds, data.Nodes, data.Ways, data.Relations);
            var indexBuildStopwatch = Stopwatch.StartNew();
            finder.FindNearByRadius(buildData, queryPoints[0].Latitude, queryPoints[0].Longitude, RadiusMeters);
            indexBuildStopwatch.Stop();

            var gridIndex = GridNodeIndex.Build(buildData.Nodes);
            var nodeOnlyStopwatch = Stopwatch.StartNew();
            foreach (var point in queryPoints)
                gridIndex.FindWithinRadius(point.Latitude, point.Longitude, RadiusMeters);
            nodeOnlyStopwatch.Stop();

            var fullData = new OsmData(data.Header, data.Bounds, data.Nodes, data.Ways, data.Relations);
            var fullStopwatch = Stopwatch.StartNew();
            foreach (var point in queryPoints)
                finder.FindNearByRadius(fullData, point.Latitude, point.Longitude, RadiusMeters);
            fullStopwatch.Stop();

            double nodeOnlyMsPerQuery = nodeOnlyStopwatch.Elapsed.TotalMilliseconds / QueryPointCount;
            double fullMsPerQuery = fullStopwatch.Elapsed.TotalMilliseconds / QueryPointCount;
            // Way/Relation gathering isn't separately timeable (GatherNodeRelatedEntities is private) - derived
            // the same way ADR-08 derives it: the full call's per-query cost minus the isolated node-search-only cost.
            double gatheringMsPerQuery = fullMsPerQuery - nodeOnlyMsPerQuery;

            // Correctness check (untimed, after all timing is captured): the manually-driven sequence above
            // must match what find_near_point itself actually returns. Reuses the already-warmed placeLookup/
            // dataSource instances, so this is a cache hit on both - no extra network calls, and no effect on
            // the timings captured above.
            var handler = new FindNearPointHandler(placeLookup, dataSource, finder, finder);
            var handlerResult = await handler.FindAsync(Place, RadiusMeters, tags: null, limit: 10);
            CollectionAssert.AreEquivalent(
                nearestAtLocation.Select(n => n.Id).ToList(),
                handlerResult.Select(r => r.Id).ToList(),
                "The manually-driven finder sequence diverged from what find_near_point itself returns.");

            // Report
            double networkMs = geocodeStopwatch.Elapsed.TotalMilliseconds + overpassStopwatch.Elapsed.TotalMilliseconds;
            TestContext.WriteLine($"Geocoding lookup ('{Place}'):                     {geocodeStopwatch.Elapsed.TotalMilliseconds:F2} ms.");
            TestContext.WriteLine($"Overpass fetch ({data.Nodes.Count} nodes, {data.Ways.Count} ways, {data.Relations.Count} relations): {overpassStopwatch.Elapsed.TotalMilliseconds:F2} ms.");
            TestContext.WriteLine($"Grid-index build (1st query, one-time cost):     {indexBuildStopwatch.Elapsed.TotalMilliseconds:F2} ms. (see #18)");
            TestContext.WriteLine($"Way/Relation gathering (derived, full - node-only): {gatheringMsPerQuery:F3} ms/query. (see #23)");
            TestContext.WriteLine($"Finder execution (one full find_near_point call, incl. grid build): {finderExecutionStopwatch.Elapsed.TotalMilliseconds:F2} ms.");
            TestContext.WriteLine($"  (reference, over {QueryPointCount} sampled queries: indexed node search alone {nodeOnlyMsPerQuery:F3} ms/query, full FindNearByRadius {fullMsPerQuery:F3} ms/query.)");
            TestContext.WriteLine($"Geocoding + Overpass fetch total: {networkMs:F2} ms, vs. finder execution {finderExecutionStopwatch.Elapsed.TotalMilliseconds:F2} ms ({finderExecutionStopwatch.Elapsed.TotalMilliseconds / networkMs:P1} of network time).");
        }

        /// <summary>
        /// Reimplements <c>FindNearPointHandler.BoundsFromRadius</c> exactly (that method is private), so this
        /// benchmark fetches over the same bounds a real find_near_point call would.
        /// </summary>
        private static OsmCoordinateBounds BoundsFromRadius(double latitude, double longitude, double radiusMeters)
        {
            const double MetersPerDegreeLatitude = 111_320d;
            const double MaxAbsLatitudeDegrees = 89.9d;

            var latitudeDelta = radiusMeters / MetersPerDegreeLatitude;
            var clampedLatitude = Math.Clamp(latitude, -MaxAbsLatitudeDegrees, MaxAbsLatitudeDegrees);
            var longitudeDelta = radiusMeters / (MetersPerDegreeLatitude * Math.Cos(clampedLatitude * Math.PI / 180d));

            return new OsmCoordinateBounds(
                Math.Max(latitude - latitudeDelta, -90d),
                Math.Max(longitude - longitudeDelta, -180d),
                Math.Min(latitude + latitudeDelta, 90d),
                Math.Min(longitude + longitudeDelta, 180d));
        }

        public TestContext TestContext { get; set; } = null!;
    }
}
