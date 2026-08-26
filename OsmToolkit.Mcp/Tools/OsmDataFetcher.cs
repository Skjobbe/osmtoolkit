using OsmToolkit.DataSources;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("OsmToolkitTests")]

namespace OsmToolkit.Mcp.Tools
{
    /// <summary>
    /// Shared boundary between the MCP handlers and <see cref="IOsmDataSource"/>: translates the data source's
    /// own fetch-failure exceptions into <see cref="OsmDataUnavailableException"/> in one place, so the three
    /// handlers can't drift into three slightly different messages if edited separately.
    /// </summary>
    internal static class OsmDataFetcher
    {
        /// <summary>
        /// Fetches OSM data for <paramref name="bounds"/> from <paramref name="dataSource"/>, wrapping any
        /// fetch-failure exception - a non-success HTTP response, an unparseable response body, or a
        /// server-side query failure, after any retry the data source itself performs has been exhausted - in
        /// an <see cref="OsmDataUnavailableException"/>.
        /// </summary>
        /// <param name="dataSource">The data source to fetch from.</param>
        /// <param name="bounds">The bounding box to fetch data for.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <exception cref="OsmDataUnavailableException">Thrown when the data source fails to fetch or parse the requested data.</exception>
        public static async Task<OsmData> FetchAsync(IOsmDataSource dataSource, OsmCoordinateBounds bounds, CancellationToken cancellationToken)
        {
            try
            {
                return await dataSource.GetOsmDataAsync(bounds, cancellationToken);
            }
            catch (Exception ex) when (ex is HttpRequestException or OverpassQueryFailedException or InvalidOperationException)
            {
                throw new OsmDataUnavailableException(
                    "OpenStreetMap data for the requested area could not be fetched right now. This is usually transient - try again shortly, or narrow the search area.",
                    ex);
            }
        }
    }
}
