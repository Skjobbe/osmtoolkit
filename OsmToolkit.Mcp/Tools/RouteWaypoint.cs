namespace OsmToolkit.Mcp.Tools
{
    /// <summary>
    /// A single point along a calculated route, in path order from origin to destination.
    /// </summary>
    /// <param name="Latitude">The waypoint's latitude.</param>
    /// <param name="Longitude">The waypoint's longitude.</param>
    public sealed record RouteWaypoint(double Latitude, double Longitude);
}
