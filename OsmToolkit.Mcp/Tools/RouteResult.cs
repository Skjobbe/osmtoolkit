namespace OsmToolkit.Mcp.Tools
{
    /// <summary>
    /// The result of a <c>route_between_points</c> search: either a calculated route, or an empty
    /// result with <see cref="Description"/> explaining why no route was found.
    /// </summary>
    /// <param name="OriginDisplayName">The resolved, human-readable name of the origin place.</param>
    /// <param name="DestinationDisplayName">The resolved, human-readable name of the destination place.</param>
    /// <param name="TravelMode">The travel mode the route was calculated for (<c>foot</c>, <c>bicycle</c>, <c>moped</c>, or <c>car</c>).</param>
    /// <param name="AvoidMotorway">Whether motorways were excluded from the route.</param>
    /// <param name="TotalDistanceMeters">The total route distance, in meters. Zero when no route was found.</param>
    /// <param name="Waypoints">The route's nodes, in order from origin to destination. Empty when no route was found.</param>
    /// <param name="Description">Set only when no route was found, describing why.</param>
    public sealed record RouteResult(
        string OriginDisplayName,
        string DestinationDisplayName,
        string TravelMode,
        bool AvoidMotorway,
        double TotalDistanceMeters,
        IReadOnlyList<RouteWaypoint> Waypoints,
        string? Description);
}
