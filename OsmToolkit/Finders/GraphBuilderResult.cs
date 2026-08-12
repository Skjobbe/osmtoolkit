namespace OsmToolkit.Finders
{
    internal record GraphBuilderResult
    (
        Dictionary<long, List<(long NeighborId, double Cost)>> Graph,
        Dictionary<long, Node> NodeDictionary,
        Dictionary<(long From, long To), Way> WaySegments
    );
}
