namespace OsmToolkit.Finders
{
    /// <summary>
    /// Defines finder methods for finding specific <typeparamref name="T"/> instances in an <see cref="OsmData"/> instance.
    /// This interface replaces <see cref="IOsmFinder{T}"/> and provides additional features.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="OsmEntity"/> instances to find.</typeparam>
    public interface IOsmFinderV2<T> : IOsmValueFinder<T>, INearestNodesFinder, IWithinDistanceFinder<T>, IShortestPathFinder  where T : OsmEntity 
    { 
    
    }
}
