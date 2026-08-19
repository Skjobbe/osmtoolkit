using OsmToolkit.DataSources;
using OsmToolkit.Finders;
using OsmToolkit.Finders.Spatial;
using System.Diagnostics;
using System.Linq;

namespace OsmToolkit.Tests.Finders
{
    /// <summary>
    /// Measures whether the grid index added in #20-#22 actually helps, against the reference point issue #2
    /// cited when the work was scoped: a linear-scan <c>FindNearByRadius</c> over all of Norway reportedly took
    /// 144 minutes and 32 GB. This fetches a real, much smaller Fredrikstad extract from the live Overpass API and
    /// times the same radius search with and without the index, rather than assuming the index helped.
    /// Excluded from CI via the TestCategory filter in .github/workflows/ci.yml, since it depends on network
    /// access and a third-party service's availability and rate limits.
    /// Run it manually with: dotnet test --filter "TestCategory=ManualIntegration"
    /// </summary>
    [TestClass]
    [TestCategory("ManualIntegration")]
    public class GridIndexBenchmarkManualTests
    {
        // A larger slice of central Fredrikstad than OverpassOsmDataSourceManualTests's cafe box - big enough
        // that a linear scan's O(n) cost is measurable, but still well under Overpass's default 10,000 km²
        // area guardrail and 25-second server-side query timeout.
        private static readonly OsmCoordinateBounds FredrikstadExtract = new(59.19, 10.92, 59.23, 10.99);

        private const double RadiusMeters = 500d;
        private const int QueryPointCount = 25;

        [TestMethod]
        public async Task FindNearByRadius_WithAndWithoutIndex_ReportsSpeedupOnFredrikstadExtract()
        {
            // Arrange
            var dataSource = new OverpassOsmDataSource();
            var data = await dataSource.GetOsmDataAsync(FredrikstadExtract);
            TestContext.WriteLine($"Fetched {data.Nodes.Count} nodes, {data.Ways.Count} ways, {data.Relations.Count} relations from the real Overpass API.");
            Assert.IsTrue(data.Nodes.Count >= QueryPointCount,
                $"Expected at least {QueryPointCount} nodes in the Fredrikstad extract to sample query points from - got {data.Nodes.Count}.");

            int step = data.Nodes.Count / QueryPointCount;
            var queryPoints = Enumerable.Range(0, QueryPointCount).Select(i => data.Nodes[i * step]).ToList();

            // Correctness check first (untimed): the indexed result must match a linear scan exactly, for every
            // query point, before any timing numbers are trusted.
            var correctnessFinder = new OsmEntityFinder();
            foreach (var point in queryPoints)
            {
                var expectedIds = LinearScanNodeIdsWithinRadius(data, point.Latitude, point.Longitude, RadiusMeters);
                var actualIds = correctnessFinder.FindNearByRadius(data, point.Latitude, point.Longitude, RadiusMeters).Nodes.Select(n => n.Id).ToList();
                CollectionAssert.AreEquivalent(expectedIds, actualIds, $"Indexed FindNearByRadius diverged from a linear scan for query node {point.Id}.");
            }

            // Act: separate OsmData instances for each side of the timed comparison, since the grid index is
            // cached per-OsmData-instance identity (see GetGridIndex) - reusing the correctness-check instance
            // would let it benefit from an index already warmed above.
            var linearData = new OsmData(data.Header, data.Bounds, data.Nodes, data.Ways, data.Relations);
            var indexedData = new OsmData(data.Header, data.Bounds, data.Nodes, data.Ways, data.Relations);
            var finder = new OsmEntityFinder();

            var linearStopwatch = Stopwatch.StartNew();
            foreach (var point in queryPoints)
                LinearScanNodeIdsWithinRadius(linearData, point.Latitude, point.Longitude, RadiusMeters);
            linearStopwatch.Stop();

            // The first indexed call pays the one-time grid build cost; reported separately from the
            // steady-state per-query cost, since a real caller building the index once and then running many
            // queries against it (the pattern the index was designed for) only pays it once.
            var indexBuildStopwatch = Stopwatch.StartNew();
            finder.FindNearByRadius(indexedData, queryPoints[0].Latitude, queryPoints[0].Longitude, RadiusMeters);
            indexBuildStopwatch.Stop();

            var indexedStopwatch = Stopwatch.StartNew();
            foreach (var point in queryPoints)
                finder.FindNearByRadius(indexedData, point.Latitude, point.Longitude, RadiusMeters);
            indexedStopwatch.Stop();

            // FindNearByRadius does two things: an (indexed) node search, then an unindexed linear scan over
            // every Way and Relation to gather the ones referencing a found node (see GatherNodeRelatedEntities -
            // explicitly out of scope for #20-#22, see issue #19's "Indexing Ways or Relations spatially" exclusion).
            // Timing the node-only search in isolation shows how much of the full-call time is actually the
            // index's doing, versus that separate, still-linear step.
            var gridIndex = GridNodeIndex.Build(indexedData.Nodes);
            var indexedNodeOnlyStopwatch = Stopwatch.StartNew();
            foreach (var point in queryPoints)
                gridIndex.FindWithinRadius(point.Latitude, point.Longitude, RadiusMeters);
            indexedNodeOnlyStopwatch.Stop();

            // Report
            double linearMsPerQuery = linearStopwatch.Elapsed.TotalMilliseconds / QueryPointCount;
            double indexedMsPerQuery = indexedStopwatch.Elapsed.TotalMilliseconds / QueryPointCount;
            double indexedNodeOnlyMsPerQuery = indexedNodeOnlyStopwatch.Elapsed.TotalMilliseconds / QueryPointCount;
            double fullCallSpeedup = linearStopwatch.Elapsed.TotalMilliseconds / Math.Max(0.001, indexedStopwatch.Elapsed.TotalMilliseconds);
            double nodeSearchSpeedup = linearStopwatch.Elapsed.TotalMilliseconds / Math.Max(0.001, indexedNodeOnlyStopwatch.Elapsed.TotalMilliseconds);

            TestContext.WriteLine($"{QueryPointCount} FindNearByRadius queries (radius {RadiusMeters:F0}m) over {data.Nodes.Count} nodes, {data.Ways.Count} ways, {data.Relations.Count} relations:");
            TestContext.WriteLine($"  Linear node scan (no index):          {linearStopwatch.Elapsed.TotalMilliseconds:F2} ms total, {linearMsPerQuery:F3} ms/query.");
            TestContext.WriteLine($"  Grid index build (1st query):         {indexBuildStopwatch.Elapsed.TotalMilliseconds:F2} ms (one-time cost, amortized over all later queries against the same OsmData instance).");
            TestContext.WriteLine($"  Indexed node search only:             {indexedNodeOnlyStopwatch.Elapsed.TotalMilliseconds:F2} ms total, {indexedNodeOnlyMsPerQuery:F3} ms/query ({nodeSearchSpeedup:F1}x vs. linear node scan).");
            TestContext.WriteLine($"  Full FindNearByRadius (indexed node search + unindexed Way/Relation gathering): {indexedStopwatch.Elapsed.TotalMilliseconds:F2} ms total, {indexedMsPerQuery:F3} ms/query ({fullCallSpeedup:F1}x vs. linear node scan).");
        }

        /// <summary>
        /// Reimplements the pre-#20 linear-scan <c>FindNearByRadius</c> algorithm exactly (a Haversine check
        /// against every node), as the "without index" side of the comparison - production code no longer has
        /// a non-indexed path to call directly.
        /// </summary>
        private static List<long> LinearScanNodeIdsWithinRadius(OsmData data, double latitude, double longitude, double radiusMeters)
        {
            var ids = new List<long>();
            foreach (var node in data.Nodes)
            {
                if (GeoDistance.HaversineMeters(latitude, longitude, node.Latitude, node.Longitude) <= radiusMeters)
                    ids.Add(node.Id);
            }
            return ids;
        }

        public TestContext TestContext { get; set; } = null!;
    }
}
