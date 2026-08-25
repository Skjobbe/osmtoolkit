using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OsmToolkit;
using OsmToolkit.Mcp.Tools;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    // The stdio transport uses stdout for the JSON-RPC stream itself, so any log output that
    // reached stdout would corrupt it. Routing everything to stderr instead keeps them separate.
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddOsmToolkit();
builder.Services.AddTransient<SearchByTagsInAreaHandler>();
builder.Services.AddTransient<FindNearPointHandler>();
builder.Services.AddTransient<RouteBetweenPointsHandler>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<SearchByTagsInAreaTool>()
    .WithTools<FindNearPointTool>()
    .WithTools<RouteBetweenPointsTool>();

await builder.Build().RunAsync();
