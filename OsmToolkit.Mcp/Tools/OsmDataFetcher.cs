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
        public static Task<OsmData> FetchAsync(IOsmDataSource dataSource, OsmCoordinateBounds bounds, CancellationToken cancellationToken) =>
            WrapFetchFailuresAsync(() => dataSource.GetOsmDataAsync(bounds, cancellationToken));

        /// <summary>
        /// Fetches OSM data for <paramref name="bounds"/> scoped to <paramref name="tags"/> from <paramref name="dataSource"/>,
        /// wrapping the same fetch-failure exceptions as the <see cref="IOsmDataSource"/> overload above.
        /// </summary>
        /// <param name="dataSource">The tag-filtered data source to fetch from.</param>
        /// <param name="bounds">The bounding box to fetch data for.</param>
        /// <param name="tags">The tag filter to scope the fetch to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <exception cref="OsmDataUnavailableException">Thrown when the data source fails to fetch or parse the requested data.</exception>
        public static Task<OsmData> FetchAsync(ITagFilteredOsmDataSource dataSource, OsmCoordinateBounds bounds, IReadOnlyDictionary<string, string?> tags, CancellationToken cancellationToken) =>
            WrapFetchFailuresAsync(() => dataSource.GetOsmDataAsync(bounds, tags, cancellationToken));

        private static async Task<OsmData> WrapFetchFailuresAsync(Func<Task<OsmData>> fetch)
        {
            try
            {
                return await fetch();
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
