namespace OsmToolkit.Mcp.Tools
{
    /// <summary>
    /// Thrown when OSM data could not be fetched for a requested area - a non-success HTTP response, an
    /// unparseable response body, or a server-side query failure reported by the underlying data source - after
    /// any retry the data source itself performs has been exhausted. Wraps the data source's own exception
    /// (<see cref="System.Net.Http.HttpRequestException"/>, <see cref="OsmToolkit.DataSources.OverpassQueryFailedException"/>,
    /// or <see cref="System.InvalidOperationException"/>) with a message suitable for surfacing directly to an
    /// MCP client, rather than leaking the source exception's HTTP/JSON-specific wording. Kept in
    /// <c>OsmToolkit.Mcp</c> rather than the core library so the core library's own exceptions stay meaningful
    /// for non-MCP consumers.
    /// </summary>
    public class OsmDataUnavailableException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OsmDataUnavailableException"/> class.
        /// </summary>
        /// <param name="message">A message suitable for surfacing directly to an MCP client.</param>
        /// <param name="innerException">The data source exception this wraps.</param>
        public OsmDataUnavailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
