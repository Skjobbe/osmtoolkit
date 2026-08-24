using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace OsmToolkit.Tests.Mcp
{
    /// <summary>
    /// Launches the real OsmToolkit.Mcp server as a subprocess and talks to it the way an actual MCP client
    /// would, over its real stdio transport - unlike <see cref="FindNearPointHandlerTests"/>, which calls
    /// the handler directly and never touches the MCP transport/attribute layer at all. Exercises the two
    /// things a handler-only unit test cannot see: that the tool is actually registered with the running
    /// server (the <c>WithTools&lt;T&gt;()</c> wiring in Program.cs), and that no log output reaches stdout and
    /// corrupts the JSON-RPC stream.
    /// Excluded from CI via the TestCategory filter in .github/workflows/ci.yml, since it depends on network
    /// access and third-party services' (Nominatim's, Overpass's) availability and rate limits, the same as
    /// <see cref="SearchByTagsInAreaManualTests"/>. Run it manually with: dotnet test --filter "TestCategory=ManualIntegration"
    /// Requires OsmToolkit.Mcp to have already been built (e.g. via a preceding `dotnet build`) - this test
    /// launches the already-built server rather than triggering a build of its own, since a `dotnet build`'s
    /// own status output would otherwise land on the same stdout the JSON-RPC stream uses.
    /// </summary>
    [TestClass]
    [TestCategory("ManualIntegration")]
    public class FindNearPointManualTests
    {
        [TestMethod]
        public async Task FindNearPoint_OverRealStdioServer_AppearsInToolListAndReturnsNodesInGamlebyen()
        {
            // Arrange
            var serverDllPath = Path.Combine(AppContext.BaseDirectory, "OsmToolkit.Mcp.dll");
            Assert.IsTrue(File.Exists(serverDllPath), $"{serverDllPath} was not found - build OsmToolkit.Mcp first.");

            var transport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "OsmToolkit.Mcp (manual test)",
                Command = "dotnet",
                Arguments = [serverDllPath],
            });

            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));

            await using var client = await McpClient.CreateAsync(transport, cancellationToken: timeout.Token);

            // Act
            var tools = await client.ListToolsAsync(cancellationToken: timeout.Token);
            var result = await client.CallToolAsync(
                "find_near_point",
                new Dictionary<string, object?>
                {
                    // A small neighborhood, not the whole municipality: GetOsmDataAsync fetches every
                    // node/way/relation in the resolved area, and a bigger area risks the public Overpass
                    // instance's own server-side query timeout for no added value to this test.
                    ["place"] = "Gamlebyen, Fredrikstad",
                    ["radiusMeters"] = 300,
                    ["limit"] = 5,
                },
                cancellationToken: timeout.Token);

            // Assert
            TestContext.WriteLine($"Tools advertised by the running server: {string.Join(", ", tools.Select(t => t.Name))}");
            Assert.IsTrue(tools.Any(t => t.Name == "find_near_point"), "find_near_point was not registered with the running MCP server.");

            var text = result.Content.OfType<TextContentBlock>().FirstOrDefault()?.Text;
            TestContext.WriteLine($"IsError: {result.IsError}, Content: {text}");
            Assert.IsFalse(result.IsError == true, "The tool call returned an error result.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(text), "Expected a non-empty result for nodes near Gamlebyen.");
        }

        public TestContext TestContext { get; set; } = null!;
    }
}
