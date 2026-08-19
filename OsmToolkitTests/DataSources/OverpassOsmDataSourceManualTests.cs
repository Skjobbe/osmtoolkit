using OsmToolkit.DataSources;
using System.Linq;

namespace OsmToolkit.Tests.DataSources
{
    /// <summary>
    /// Talks to the real, public Overpass API instead of a fake HttpMessageHandler, unlike every other
    /// OverpassOsmDataSource test. Excluded from CI via the TestCategory filter in .github/workflows/ci.yml,
    /// since it depends on network access and a third-party service's availability and rate limits.
    /// Run it manually with: dotnet test --filter "TestCategory=ManualIntegration"
    /// </summary>
    [TestClass]
    [TestCategory("ManualIntegration")]
    public class OverpassOsmDataSourceManualTests
    {
        // A few blocks of central Fredrikstad, Norway - kept deliberately small (not the whole city
        // center) since GetOsmDataAsync fetches every node/way/relation in the box, not just cafes;
        // a wider box risks the public Overpass instance's own rate limits/timeouts for no added value here.
        private static readonly OsmCoordinateBounds FredrikstadCityCenter = new(59.200, 10.948, 59.207, 10.958);

        [TestMethod]
        public async Task GetOsmDataAsync_CafesInFredrikstad_ReturnsCafesFromRealOverpassApi()
        {
            // Arrange
            var dataSource = new OverpassOsmDataSource();

            // Act
            var data = await dataSource.GetOsmDataAsync(FredrikstadCityCenter);
            var cafes = data.Nodes
                .Where(n => n.Tags.TryGetValue("amenity", out var value) && value == "cafe")
                .ToList();

            // Assert
            TestContext.WriteLine($"Fetched {data.Nodes.Count} nodes, {data.Ways.Count} ways, {data.Relations.Count} relations from the real Overpass API.");
            TestContext.WriteLine($"Cafes found in central Fredrikstad: {cafes.Count}.");
            foreach (var cafe in cafes)
            {
                cafe.Tags.TryGetValue("name", out var name);
                TestContext.WriteLine($"  - id {cafe.Id}: {name ?? "(unnamed)"}");
            }

            Assert.IsTrue(cafes.Count > 0, "Expected at least one cafe in central Fredrikstad from the real Overpass API.");
        }

        public TestContext TestContext { get; set; } = null!;
    }
}
