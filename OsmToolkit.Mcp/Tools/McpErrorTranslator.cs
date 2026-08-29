using ModelContextProtocol;
using OsmToolkit.Geocoding;

namespace OsmToolkit.Mcp.Tools
{
    /// <summary>
    /// Translates OsmToolkit.Mcp's own deliberately-thrown, client-safe exceptions
    /// (<see cref="PlaceNotFoundException"/>, <see cref="OsmDataUnavailableException"/>) into
    /// <see cref="McpException"/>, whose message the MCP SDK forwards to the calling client as-is. Any other
    /// exception is left to propagate untouched, so it still falls through to the SDK's default
    /// unhandled-exception behavior - a generic, detail-free error result - rather than risking an
    /// unanticipated internal exception's message leaking to a remote client.
    /// </summary>
    internal static class McpErrorTranslator
    {
        /// <summary>
        /// Invokes <paramref name="operation"/>, translating known, client-safe failures into <see cref="McpException"/>.
        /// </summary>
        public static async Task<T> TranslateKnownFailuresAsync<T>(Func<Task<T>> operation)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (ex is PlaceNotFoundException or OsmDataUnavailableException)
            {
                throw new McpException(ex.Message, ex);
            }
        }
    }
}
